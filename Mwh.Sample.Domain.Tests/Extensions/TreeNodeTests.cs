namespace Mwh.Sample.Domain.Tests.Extensions;

[TestClass]
public class TreeNodeTests
{
    [TestMethod]
    public void AddChild_ShouldAddChildNodeInSortedOrder()
    {
        // Arrange
        var rootNode = new TreeNode<int>(5);

        // Act
        var child1 = rootNode.AddChild(3);
        child1.AddChild(30);
        child1.AddChild(31);

        var child2 = rootNode.AddChild(7);
        child2.AddChild(70);
        child2.AddChild(71);
        child2.AddChild(72);


        var child3 = rootNode.AddChild(1);
        child3.AddChild(10);
        child3.AddChild(11);
        child3.AddChild(12);

        var test = rootNode.EnumerateSelfAndDescendants().ToList();
        var test2 = rootNode.EnumerateSelfAndDescendantsWithDepth();


        // Assert
        Assert.AreEqual(12, rootNode.EnumerateSelfAndDescendants().Count());
        Assert.AreEqual(5, rootNode.EnumerateSelfAndDescendants().ElementAt(0));
        Assert.AreEqual(1, rootNode.EnumerateSelfAndDescendants().ElementAt(1));
        Assert.AreEqual(10, rootNode.EnumerateSelfAndDescendants().ElementAt(2));
        Assert.AreEqual(0, rootNode.EnumerateSelfAndDescendantsWithDepth().ElementAt(0).Depth);
        Assert.AreEqual(1, rootNode.EnumerateSelfAndDescendantsWithDepth().ElementAt(1).Value);
    }

    [TestMethod]
    public void EnumerateSelfAndDescendants_ShouldReturnCorrectDepthAndValues()
    {
        // Arrange
        var rootNode = new TreeNode<string>("A");
        var child1 = rootNode.AddChild("B");
        var child2 = rootNode.AddChild("C");
        var child3 = child1.AddChild("D");
        var child4 = child1.AddChild("E");

        // Act
        var result = rootNode.EnumerateSelfAndDescendantsWithDepth().ToList();

        // Assert
        Assert.AreEqual(5, result.Count);
        Assert.AreEqual(0, result[0].Depth);
        Assert.AreEqual("A", result[0].Value);
        Assert.AreEqual(1, result[1].Depth);
        Assert.AreEqual("B", result[1].Value);
        Assert.AreEqual(2, result[2].Depth);
        Assert.AreEqual("D", result[2].Value);
        Assert.AreEqual(2, result[3].Depth);
        Assert.AreEqual("E", result[3].Value);
        Assert.AreEqual(1, result[4].Depth);
        Assert.AreEqual("C", result[4].Value);
    }
}





