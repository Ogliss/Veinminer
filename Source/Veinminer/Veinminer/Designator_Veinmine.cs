using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Veinminer
{
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

		protected override DesignationDef Designation
		{
			get
			{
				return DesignationDefOf.Mine;
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
			if(base.Map.designationManager.DesignationAt(c, DesignationDefOf.Mine) != null ) 
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
			if(base.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) != null) 
			{
				return AcceptanceReport.WasRejected;
			}
			return true;
		}

		public override void DesignateSingleCell(IntVec3 loc) 
		{
			base.Map.designationManager.AddDesignation(new Designation(loc, DesignationDefOf.Mine));
			numDesignated = 0;
			CheckNearby(loc);
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
						if(base.Map.designationManager.DesignationAt(nearbyLocs[i], DesignationDefOf.Mine) != null) 
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

		protected override void FinalizeDesignationSucceeded() 
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
