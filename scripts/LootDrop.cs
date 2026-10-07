// LootDrop.cs (excerpt)
public void Drop()
{
    // Assume `ItemTemplate` is an exported Resource reference.
    var generatedItem = AffixGenerator.Roll(ItemTemplate);
    // Spawn the item in the world, assign its data, etc.
    var instance = (PackedScene)ItemScene.Instance();
    var itemNode = instance.GetNode<ItemNode>("Item");
    itemNode.Setup(generatedItem);
    GetParent().AddChild(instance);
}
