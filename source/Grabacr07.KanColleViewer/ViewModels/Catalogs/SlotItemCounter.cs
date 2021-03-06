﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Properties;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.ViewModels.Catalogs
{
	public struct SlotItemCounterKey
	{
		public int Level { get; set; }
		public int Adept { get; set; }
		public SlotItemCounterKey(int level, int adept) : this()
		{
			this.Level = level;
			this.Adept = adept;
		}
	}

	public class SlotItemCounter
	{
		// Key:   装備レベル (★), 艦載機熟練度
		// Value: レベル別の装備数カウンター
		private readonly Dictionary<SlotItemCounterKey, SlotItemCounterByLevel> itemsByLevel;

		public SlotItemInfo Target { get; private set; }

		public IReadOnlyCollection<SlotItemCounterByLevel> Levels
		{
			get
			{
				return this.itemsByLevel
					.OrderBy(x => x.Key.Level)
					.ThenBy(x => x.Key.Adept)
					.Select(x => x.Value)
					.ToList();
			}
		}

		public int Count => this.itemsByLevel.Sum(x => x.Value.Count);


		public SlotItemCounter(SlotItemInfo target, IEnumerable<SlotItem> items)
		{
			this.Target = target;

			this.itemsByLevel = items
				.GroupBy(x => new SlotItemCounterKey(x.Level, x.Proficiency))
				.ToDictionary(
					x => x.Key,
					x => new SlotItemCounterByLevel { CounterKey = x.Key, Count = x.Count(), }
				);
		}

		public void AddShip(Ship ship, int itemLevel, int adept)
		{
			this.itemsByLevel[new SlotItemCounterKey(itemLevel, adept)].AddShip(ship);
		}

		public string Stats
		{
			get
			{
				var addDetail = "";

				if (this.Target.Firepower != 0) addDetail += " " + ToSignedString(this.Target.Firepower) + " " + Resources.Stats_Firepower;
				if (this.Target.AA != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.AA) + " " + Resources.Stats_AntiAir;
				if (this.Target.Torpedo != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.Torpedo) + " " + Resources.Stats_Torpedo;
				if (this.Target.ASW != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.ASW) + " " + Resources.Stats_AntiSub;
				if (this.Target.ViewRange != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.ViewRange) + " " + Resources.Stats_SightRange;
				if (this.Target.Armer != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.Armer) + " " + Resources.Stats_Armor;
				if (this.Target.Evade != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.Evade) + " " + Resources.Stats_Evasion;
				if (this.Target.Hit != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.Hit) + " " + Resources.Stats_Accuracy;
				if (this.Target.Bomb != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + ToSignedString(this.Target.Bomb) + " " + Resources.Stats_DiveBomb;
				if (this.Target.AttackRange != 0) addDetail += (addDetail != "" ? "\n" : "") + " " + IntToRange(this.Target.AttackRange) + " " + Resources.Stats_AttackRange;

				return addDetail;
			}
		}

		private string ToSignedString(int value) => value.ToString("+#;-#;#");

		private string IntToRange(int value)
		{
			switch (value)
			{
				case 1:
					return Resources.SlotItemCatalog_AttackRange_Short;
				case 2:
					return Resources.SlotItemCatalog_AttackRange_Medium;
				case 3:
					return Resources.SlotItemCatalog_AttackRange_Long;
				case 4:
					return Resources.SlotItemCatalog_AttackRange_VeryLong;
				default:
					return Resources.SlotItemCatalog_AttackRange_Unknown;
			}
		}
	}


	public class SlotItemCounterByLevel
	{
		// Key:   艦娘の ID
		// Value: 艦娘別の装備カウンター
		private readonly Dictionary<int, SlotItemCounterByShip> itemsByShip;
		private int count;

		public SlotItemCounterKey CounterKey { get; set; }

		public IReadOnlyCollection<SlotItemCounterByShip> Ships
		{
			get
			{
				return this.itemsByShip
					.Values
					.OrderByDescending(x => x.Ship.Level)
					.ThenBy(x => x.Ship.SortNumber).ToList();
			}
		}

		public int Count
		{
			get { return this.count; }
			set { this.count = this.Remainder = value; }
		}

		// 余り
		public int Remainder { get; private set; }


		public SlotItemCounterByLevel()
		{
			this.itemsByShip = new Dictionary<int, SlotItemCounterByShip>();
		}

		public void AddShip(Ship ship)
		{
			SlotItemCounterByShip target;
			if (this.itemsByShip.TryGetValue(ship.Id, out target))
			{
				target.Count++;
			}
			else
			{
				this.itemsByShip.Add(ship.Id, new SlotItemCounterByShip { Ship = ship, Count = 1 });
			}

			this.Remainder--;
		}
	}

	public class SlotItemCounterByShip
	{
		public Ship Ship { get; set; }

		public int Count { get; set; }

		public string ShipName => this.Ship.Info.Name;

		public string ShipLevel => "Lv." + this.Ship.Level;

		public string CountString => this.Count == 1 ? "" : " x " + this.Count + " ";
	}
}
