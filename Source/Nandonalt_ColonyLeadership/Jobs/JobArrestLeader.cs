using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Nandonalt_ColonyLeadership;

public class JobArrestLeader : JobDriver
{
    private const TargetIndex TakeeIndex = TargetIndex.A;

    private const TargetIndex BedIndex = TargetIndex.B;

    protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    protected Building_Bed DropBed => (Building_Bed)job.GetTarget(TargetIndex.B).Thing;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOnAggroMentalState(TargetIndex.A);
        if (job.def == JobDefOf.Rescue)
        {
            this.FailOnNotDowned(TargetIndex.A);
        }

        this.FailOn(delegate
        {
            if (job.def.makeTargetPrisoner)
            {
                if (!DropBed.ForPrisoners)
                {
                    return true;
                }
            }
            else if (DropBed.ForPrisoners != ((Pawn)(Thing)TargetA).IsPrisoner)
            {
                return true;
            }

            return false;
        });
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.B, DropBed.SleepingSlotsCount);
        yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
        globalFinishActions.Add(delegate
        {
            if (job.def.makeTargetPrisoner && Takee.ownership.OwnedBed == DropBed &&
                Takee.Position != RestUtility.GetBedSleepingSlotPosFor(Takee, DropBed))
            {
                Takee.ownership.UnclaimBed();
            }
        }); //Pretty sure I just changed this code to say If(!CanBeArrestedByMyself){... See !this.Takee.CanBeArrestedBy((Pawn)base.pawn)). Previously used to be !this.CanBeArrested() but this method no longer exists. 
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOn(() => job.def == JobDefOf.Arrest && !Takee.CanBeArrestedBy(pawn))
            .FailOn(() => !pawn.CanReach(DropBed, PathEndMode.OnCell, Danger.Deadly))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate
            {
                if (!job.def.makeTargetPrisoner)
                {
                    return;
                }

                var victim = (Pawn)job.targetA.Thing;
                var lord = victim.GetLord();
                lord?.Notify_PawnAttemptArrested(victim);


                GenClamor.DoClamor(victim, 10f, ClamorDefOf.Harm);


                if (!(Rand.Value < 0.1f))
                {
                    return;
                }

                Messages.Message("MessageRefusedArrest".Translate(victim.LabelShort), victim,
                    MessageTypeDefOf.NegativeEvent);

                victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                IncidentWorker_Rebellion.removeLeadership(victim);
                victim.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderArrested"));
                pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                Find.LetterStack.ReceiveLetter("LeaderEndLetter".Translate(),
                    "LeaderEndLetterDesc".Translate(victim.Name.ToStringFull),
                    LetterDefOf.NegativeEvent, victim);
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
        };

        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
        yield return new Toil
        {
            initAction = delegate
            {
                if (job.def.makeTargetPrisoner)
                {
                    pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                    IncidentWorker_Rebellion.removeLeadership(Takee);
                    Takee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderArrested"));
                    Find.LetterStack.ReceiveLetter("LeaderEndLetterArrested".Translate(),
                        "LeaderEndLetterDescArrested".Translate(Takee.Name.ToStringFull),
                        LetterDefOf.NegativeEvent, pawn);
                    foreach (var p in IncidentWorker_SetLeadership.getAllColonists())
                    {
                        if (p != Takee)
                        {
                            p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderArrestedColonist"));
                        }
                    }

                    if (Takee.guest.Released)
                    {
                        Takee.guest.Released = false;
                        Takee.guest.SetNoInteraction();
                    }

                    if (!Takee.IsPrisonerOfColony)
                    {
                        Takee.Faction?.Notify_MemberCaptured(Takee, pawn.Faction);

                        Takee.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
                        if (Takee.guest.IsPrisoner)
                        {
                            TaleRecorder.RecordTale(TaleDefOf.Captured, pawn, Takee);
                            pawn.records.Increment(RecordDefOf.PeopleCaptured);
                        }
                    }
                }
                else if (Takee.Faction != Faction.OfPlayer && Takee.HostFaction != Faction.OfPlayer &&
                         Takee.guest != null)
                {
                    Takee.guest.SetGuestStatus(Faction.OfPlayer);
                }

                if (Takee.playerSettings == null)
                {
                    Takee.playerSettings = new Pawn_PlayerSettings(Takee);
                }
            }
        };
        yield return Toils_Reserve.Release(TargetIndex.B);
        yield return new Toil
        {
            initAction = delegate
            {
                var position = DropBed.Position;
                pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out _);
                if (!DropBed.Destroyed && (DropBed.OwnersForReading.Contains(Takee) ||
                                           DropBed.Medical && DropBed.AnyUnoccupiedSleepingSlot ||
                                           Takee.ownership == null))
                {
                    Takee.jobs.Notify_TuckedIntoBed(DropBed);
                    if (Takee.RaceProps.Humanlike && job.def != JobDefOf.Arrest && !Takee.IsPrisonerOfColony)
                    {
                        Takee.relations.Notify_RescuedBy(pawn);
                    }
                }

                if (Takee.IsPrisonerOfColony)
                {
                    LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, Takee, OpportunityType.GoodToKnow);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }
}