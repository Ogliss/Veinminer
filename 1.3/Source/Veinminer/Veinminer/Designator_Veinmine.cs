using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Veinminer
{
	[StaticConstructorOnStartup]
	public static class VeinMinter
	{
		static VeinMinter()
		{
			Mine = DefDatabase<DesignationDef>.GetNamedSilentFail("drm_SmartMine") ?? DesignationDefOf.Mine;
        }
		public static DesignationDef Mine;
    }

    public class Designator_Veinmine: Designator_Mine
	{
		int numDesignated = 0;
		public override int DraggableDimensions 
		{
			get 
			{
				return 0;
			}
		}

		public override bool DragDrawMeasurements 
		{
			get 
			{
				return true;
			}
		}

		public override DesignationDef Designation
		{
			get
			{
				return VeinMinter.Mine;
			}
		}
		public Designator_Veinmine() 
		{
			this.defaultLabel = "DesignatorVeinmine".Translate();
			this.icon = ContentFinder<Texture2D>.Get("Designators/Veinmine", true);
			this.defaultDesc = "DesignatorVeinmineDesc".Translate();
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.Designate_Mine;
			this.tutorTag = "Mine";
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c) 
		{
			if(!c.InBounds(base.Map)) 
			{
				return false;
			}
			if(base.Map.designationManager.DesignationAt(c, Designation) != null ) 
			{
				return AcceptanceReport.WasRejected;
			}
			if(c.Fogged(base.Map)) 
			{
				return false;
			}
			Thing thing = c.GetFirstMineable(base.Map);
			if(thing == null) 
			{
				return "MessageMustDesignateMineable".Translate();
			}
			AcceptanceReport result = this.CanDesignateThing(thing);
			if(!result.Accepted) 
			{
				return result;
			}
			return AcceptanceReport.WasAccepted;
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if(!t.def.mineable) 
			{
				return false;
			}
			if(base.Map.designationManager.DesignationAt(t.Position, Designation) != null) 
			{
				return AcceptanceReport.WasRejected;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc) 
		{
			base.Map.designationManager.AddDesignation(new Designation(loc, Designation));
			numDesignated = 0;
			CheckNearby_newTemp(loc);
		}

		void CheckNearby_newTemp(IntVec3 loc)
		{
			Thing locThing = loc.GetFirstMineable(base.Map);
            if (locThing== null)
            {
				return;
            }
			List<IntVec3> cells = new List<IntVec3>();
			CheckCellsAdjacent(loc, locThing.def, ref cells);
			for (int i = 0; i < cells.Count; i++)
			{
				base.Map.designationManager.AddDesignation(new Designation(cells[i], Designation));
				numDesignated++;
				CheckCellsAdjacent(cells[i], locThing.def, ref cells);
			}
			Log.Message("cells(" + cells.Count + "," + locThing.def + ")");

		}

		void CheckCellsAdjacent(IntVec3 loc, ThingDef def, ref List<IntVec3> cells)
		{
			foreach (IntVec3 loc2 in GenAdjFast.AdjacentCells8Way(loc))
			{
				if (loc == loc2 || cells.Contains(loc2) || base.Map.designationManager.DesignationAt(loc2, Designation) != null)
				{
					continue;
				}
				if (loc2.InBounds(base.Map))
					if (loc2.GetFirstMineable(base.Map) is Thing locThing)
					{
						if (locThing.def == def)
						{
							AcceptanceReport result = this.CanDesignateThing(locThing);
							if (result.Accepted)
							{
								cells.Add(loc2);
							}
						}
					}
			}
		}

		void CheckNearby(IntVec3 loc) 
		{
			
			Thing locThing = loc.GetFirstMineable(base.Map);
			IntVec3[] nearbyLocs = new IntVec3[8];
			nearbyLocs[0] = loc + new IntVec3(1, 0, 0);
			nearbyLocs[1] = loc + new IntVec3(-1, 0, 0);
			nearbyLocs[2] = loc + new IntVec3(0, 0, 1);
			nearbyLocs[3] = loc + new IntVec3(0, 0, -1);
			nearbyLocs[4] = loc + new IntVec3(1, 0, 1);
			nearbyLocs[5] = loc + new IntVec3(1, 0, -1);
			nearbyLocs[6] = loc + new IntVec3(-1, 0, 1);
			nearbyLocs[7] = loc + new IntVec3(-1, 0, -1);

			for(int i = 0; i < nearbyLocs.Length; i++) 
			{
				if(numDesignated < 66) 
				{
					if(nearbyLocs[i].InBounds(base.Map)) 
					{
						bool canDesignate = true;
						if(base.Map.designationManager.DesignationAt(nearbyLocs[i], Designation) != null) 
						{
							canDesignate = false;
						}
						Thing nearbyLocThing = nearbyLocs[i].GetFirstMineable(base.Map);
						if(nearbyLocThing == null) 
						{
							canDesignate = false;
						} else if(locThing.DescriptionFlavor != nearbyLocThing.DescriptionFlavor) 
						{
							canDesignate = false;
						}
						if(nearbyLocThing != null) 
						{
							AcceptanceReport result = this.CanDesignateThing(nearbyLocThing);
							if(!result.Accepted) 
							{
								canDesignate = false;
							}
						}
						if(canDesignate) 
						{
							this.DesignateSingleCell(nearbyLocs[i]);
							numDesignated++;
						}
					}
				}
			}
		}

		public override void DesignateThing(Thing t) 
		{
			this.DesignateSingleCell(t.Position);
		}

		public override void FinalizeDesignationSucceeded() 
		{
			base.FinalizeDesignationSucceeded();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Mining, KnowledgeAmount.SpecificInteraction);
		}

		public override void SelectedUpdate() 
		{
			GenUI.RenderMouseoverBracket();
		}

	}
}
