using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Characters;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using System;

public partial class HUD : Control
{
    public ProgressBar healthBar;
    public ProgressBar manaBar;
    public ProgressBar castBar;

    public PanelContainer targetUnitFrame;
    public ProgressBar targetHealthBar;
    public ProgressBar targetManaBar;

    public Panel inventoryPanel;
    public ItemList itemList;

    public Panel lootPanel;
    public ItemList lootList;

    private ItemList spellBar;

    Player3D player;
    public Panel charSheetPanel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        player = GetParent<Player3D>();
        itemList = GetNode<ItemList>("InventoryPanel/ItemList");
        inventoryPanel = GetNode<Panel>("InventoryPanel");
        itemList = GetNode<ItemList>("InventoryPanel/ItemList");
        castBar = GetNode<ProgressBar>("CastBar");
        spellBar = GetNode<ItemList>("SpellBar/SpellList");

        lootPanel = GetNode<Panel>("LootPanel");
        lootList = GetNode<ItemList>("LootPanel/LootList");

        healthBar = GetNode<ProgressBar>("PlayerUnitFrame/Grid/HealthBar");
        manaBar = GetNode<ProgressBar>("PlayerUnitFrame/Grid/ManaBar");

        targetUnitFrame = GetNode<PanelContainer>("TargetUnitFrame");
        targetHealthBar = GetNode<ProgressBar>("TargetUnitFrame/Grid/HealthBar");
        targetManaBar = GetNode<ProgressBar>("TargetUnitFrame/Grid/ManaBar");

        charSheetPanel = GetNode<Panel>("CharacterPanel");

        UpdateUnitFrame();
        ReadyCharacterSheetPanel();
        ReadySpells();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (Input.IsActionJustPressed("character_sheet"))
        {
            if (!charSheetPanel.Visible)
            {
                charSheetPanel.Visible = true;
            }
            else
            {
                charSheetPanel.Visible = false;
            }
        }

        if (Input.IsActionJustPressed("inventory"))
        {
            if (!inventoryPanel.Visible)
            {
                inventoryPanel.Visible = true;
            }
            else
            {
                inventoryPanel.Visible = false;
            }
        }
    }

    public void ReadyCharacterSheetPanel()
    {
        charSheetPanel.GetNode<Label>("Stats/MaxHealthValue").Text = $"{player.characterSheet.MaxHealth.Value}";
        charSheetPanel.GetNode<Label>("Stats/ArmorValue").Text = $"{player.characterSheet.Armor.Value}";
        charSheetPanel.GetNode<Label>("Stats/MeleeDamageValue").Text = $"{player.characterSheet.WeaponDamage.Value}";
        charSheetPanel.GetNode<TextureButton>("EquipmentL/HeadSlot").TextureNormal = GD.Load<Texture2D>(player.characterSheet.Equipment.Head.Icon);
        charSheetPanel.GetNode<TextureButton>("EquipmentL/BodySlot").TextureNormal = GD.Load<Texture2D>(player.characterSheet.Equipment.Body.Icon);
        charSheetPanel.GetNode<TextureButton>("Weapons/MainHand").TextureNormal = GD.Load<Texture2D>(player.characterSheet.Equipment.Melee.Icon);
    }

    public void UpdateUnitFrame()
    {
        healthBar.Value = player.currentHp;
        manaBar.Value = player.currentMana;
    }

    public void ReadySpells()
    {
        foreach (var spell in player.spellList)
        {
            spellBar.AddItem("", GD.Load<Texture2D>(spell.Icon));
        }
    }

    public void _OnItemListItemClicked(int index, Vector2 atPosition, int mouseButtonIndex)
	{
		if (mouseButtonIndex == 2)
		{
			var itemSelected = player.characterSheet.inventory.Items[index];

			if (itemSelected.GetType() == typeof(Consumable))
			{
				var consumable = (Consumable)itemSelected;
				player.UseConsumable(consumable);
				player.characterSheet.inventory.Items.Remove(itemSelected);
				itemList.RemoveItem(index);
				GD.Print($"{consumable.Name} used!");
			}
			if (itemSelected.GetType() == typeof(Equippable))
			{
				var equippable = (Equippable)itemSelected;
				player.PutOnEquipment(equippable);
                player.characterSheet.inventory.Items.Remove(itemSelected);
                itemList.RemoveItem(index);
                GD.Print($"{equippable.Name} equipped!");
            }
		}
	}

    public void _OnLootListItemClicked(int index, Vector2 atPosition, int mouseButtonIndex)
    {
        if (mouseButtonIndex == 2)
        {
			GD.Print(index);
            var itemSelected = player.lootingList[index];

            if (itemSelected.GetType() == typeof(Consumable))
            {
                var consumable = (Consumable)itemSelected;
				player.characterSheet.inventory.Items.Add(consumable);
				player.lootingList.Remove(itemSelected);
				itemList.AddItem(consumable.Name, GD.Load<Texture2D>(consumable.Icon));
				lootList.RemoveItem(index);
            }
            if (itemSelected.GetType() == typeof(Equippable))
            {
                var equippable = (Equippable)itemSelected;
				player.characterSheet.inventory.Items.Add(equippable);
				player.lootingList.Remove(itemSelected);
                itemList.AddItem(equippable.Name, GD.Load<Texture2D>(equippable.Icon));
                lootList.RemoveItem(index);
            }
        }
    }
}
