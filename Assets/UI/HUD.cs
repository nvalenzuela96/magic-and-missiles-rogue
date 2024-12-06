using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Characters;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using System;

public partial class HUD : Control
{
	ItemList itemList;
	Inventory inventory;
	Player3D player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		itemList = GetNode<ItemList>("InventoryPanel/ItemList");
		player = GetParent<Player3D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _OnItemListItemClicked(int index, Vector2 atPosition, int mouseButtonIndex)
	{
		if (mouseButtonIndex == 2)
		{
			var itemSelected = player.inventory.Items[index];

			if (itemSelected.GetType() == typeof(Consumable))
			{
				var consumable = (Consumable)itemSelected;
				player.UseConsumable(consumable);
				player.inventory.Items.Remove(itemSelected);
				itemList.RemoveItem(index);
				GD.Print($"{consumable.Name} used!");
			}
			if (itemSelected.GetType() == typeof(Equippable))
			{
				var equippable = (Equippable)itemSelected;
				player.PutOnEquipment(equippable);
                player.inventory.Items.Remove(itemSelected);
                itemList.RemoveItem(index);
                GD.Print($"{equippable.Name} equipped!");
            }
		}
	}
}
