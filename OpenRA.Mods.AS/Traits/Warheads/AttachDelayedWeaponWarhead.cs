﻿using System.Collections.Generic;
using OpenRA.GameRules;
using OpenRA.Mods.AS.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.AS.Warheads
{
	public class AttachDelayedWeaponWarhead : WarheadAS, IRulesetLoaded<WeaponInfo>
	{
		[WeaponReference]
		public readonly string Weapon = "";

		[Desc("Type listed under Types in AttachableTypes: trait of warhead that can attach to this).")]
		public readonly string Type = "bomb";

		[Desc("Range of targets to be attached.")]
		public readonly WDist Range = new WDist(1024);

		[Desc("Trigger weapon after x ticks.")]
		public readonly int TriggerTimer = 100;

		[Desc("DeathType(s) that trigger the weapon. Leave empty to always trigger the weapon on death.")]
		public readonly HashSet<string> DeathTypes = new HashSet<string>();

		public WeaponInfo WeaponInfo;

		public void RulesetLoaded(Ruleset rules, WeaponInfo info)
		{
			if (!rules.Weapons.TryGetValue(Weapon.ToLowerInvariant(), out WeaponInfo))
				throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(Weapon.ToLowerInvariant()));
		}

		public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers)
		{
			var pos = target.CenterPosition;

			if (!IsValidImpact(pos, firedBy))
				return;
			
			var availableActors = firedBy.World.FindActorsInCircle(pos, Range + VictimScanRadius);
			foreach (var actor in availableActors)
			{
				if (!IsValidAgainst(actor, firedBy))
					continue;

				if (actor.IsDead)
					continue;

				var attachable = actor.TraitOrDefault<DelayedWeaponAttachable>();
				attachable.Attach(new DelayedWeaponTrigger(TriggerTimer, DeathTypes, WeaponInfo, firedBy));
			}
		}

		public override bool IsValidAgainst(Actor victim, Actor firedBy)
		{
			var attachable = victim.TraitOrDefault<DelayedWeaponAttachable>();
			if (attachable == null || !attachable.CanAttach(Type))
				return false;

			return base.IsValidAgainst(victim, firedBy);
		}
	}
}
