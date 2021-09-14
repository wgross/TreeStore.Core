using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class LeafNodeTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mocks.VerifyAll();

        private LeafNode ArrangeNode(string name, object data) => LeafNodeFactory.Create(name, data);

        [Fact]
        public void LeafNode_rejects_null_data()
        {
            // ACT & ASSERT
            var node = Assert.Throws<ArgumentNullException>(() => this.ArrangeNode("name", null));
        }

        #region Name

        [Fact]
        public void LeafNode_rejects_null_name()
        {
            // ACT & ASSERT
            var node = Assert.Throws<ArgumentNullException>(() => this.ArrangeNode(null, new { }));
        }

        [Fact]
        public void LeafNode_provides_name()
        {
            // ARRANGE
            var node = new LeafNode(name: "name", new { });

            // ACT
            var result = node.Name;

            // ASSERT
            Assert.Equal("name", result);
        }

        #endregion Name

        #region IGetItem

        [Fact]
        public void Invoke_GetItem_creates_PSObject_of_underlying()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Equal("name", result.Property<string>("PSChildName"));
            Assert.Equal("data", result.Property<string>("Data"));
        }

        [Fact]
        public void Invoke_GetItemParameters_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var getItem = this.mocks.Create<IGetItem>();
            getItem
                .Setup(gi => gi.GetItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", getItem.Object);

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_GetItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void Invoke_GetItem_at_underlying_on_ToPSObject()
        {
            // ARRANGE

            var psObject = new PSObject();
            var getItem = this.mocks.Create<IGetItem>();
            getItem
                .Setup(gi => gi.GetItem())
                .Returns(psObject);

            var node = this.ArrangeNode("name", getItem.Object);

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Same(psObject, result);
            Assert.Equal("name", psObject.Property<string>("PSChildName"));
        }

        [Fact]
        public void Invoke_GetItem_at_Underlying()
        {
            // ARRANGE
            var psObject = new PSObject();
            var getItem = this.mocks.Create<IGetItem>();
            getItem
                .Setup(gi => gi.GetItem())
                .Returns(psObject);

            var node = this.ArrangeNode("name", getItem.Object);

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Same(psObject, result);
            Assert.Equal("name", psObject.Property<string>("PSChildName"));
        }

        #endregion IGetItem

        #region IClearItemProperty

        [Fact]
        public void Invoke_ClearItemProperty_at_underlying()
        {
            // ARRANGE
            var clearItemProperty = this.mocks.Create<IClearItemProperty>();
            clearItemProperty
                .Setup(cip => cip.ClearItemProperty(new[] { "property" }));

            var node = new ContainerNode("name", clearItemProperty.Object);

            // ACT
            node.ClearItemProperty(new[] { "property" });
        }

        [Fact]
        public void Invoke_ClearItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new ContainerNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.ClearItemProperty(new[] { "property" }));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IClearItemProperty'.", result.Message);
        }

        [Fact]
        public void Invoke_ClearItemPropertyParameters_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var clearItemProperty = this.mocks.Create<IClearItemProperty>();
            clearItemProperty
                .Setup(cip => cip.ClearItemPropertyParameters(new[] { "property" }))
                .Returns(parameters);

            var node = new ContainerNode("name", clearItemProperty.Object);

            // ACT
            var result = node.ClearItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_ClearItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var node = new ContainerNode("name", new { });

            // ACT
            var result = node.ClearItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Null(result);
        }

        #endregion IClearItemProperty

        #region IGetItemProperty

        [Fact]
        public void Invoke_GetItemProperty_projects_GetItem()
        {
            // ARRANGE
            var node = new LeafNode("name", new
            {
                Property = 1,
                SecondProperty = "text"
            });

            // ACT
            var result = node.GetItemProperty(new[] { "property" });

            // ASSERT
            Assert.Equal(1, result.Properties.Single(p => p.Name.Equals("Property")).Value);
        }

        [Fact]
        public void Invoke_GetItemProperty_without_picklist_return_complete_object()
        {
            // ARRANGE
            var node = new LeafNode("name", new
            {
                Property = 1,
                SecondProperty = "text"
            });

            // ACT

            var result = node.GetItemProperty(null);

            // ASSERT
            Assert.Equal("name", result.Properties.ElementAt(0).Value);
            Assert.Equal(1, result.Properties.ElementAt(1).Value);
            Assert.Equal("text", result.Properties.ElementAt(2).Value);
        }

        [Fact]
        public void Invoke_GetItemProperty_ignores_unkown_properties()
        {
            // ARRANGE
            var node = new LeafNode("name", new
            {
                Property = 1,
                SecondProperty = "text"
            });

            // ACT

            var result = node.GetItemProperty(new[] { "property", "unknown" });

            // ASSERT
            Assert.Equal(1, result.Properties.Single().Value);
        }

        [Fact]
        public void Invoke_GetItemProperty_invokes_underlying()
        {
            // ARRANGE
            var getItemProperty = this.mocks.Create<IGetItemProperty>();
            getItemProperty
                .Setup(gip => gip.GetItemProperty(new[] { "property" }))
                .Returns(new PSObject());

            var node = new LeafNode("name", getItemProperty.Object);

            // ACT
            var result = node.GetItemProperty(new[] { "property" });
        }

        [Fact]
        public void Invoke_GetItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var getItemProperty = this.mocks.Create<IGetItemProperty>();
            getItemProperty
                .Setup(gip => gip.GetItemPropertyParameters(new[] { "property" }))
                .Returns(parameters);

            var node = new LeafNode("name", getItemProperty.Object);

            // ACT
            var result = node.GetItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_GetItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var node = new LeafNode("name", new { });

            // ACT
            var result = node.GetItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Null(result);
        }

        #endregion IGetItemProperty

        #region ISetItemProperty

        [Fact]
        public void SetItemProperty_invokes_underlying()
        {
            // ARRANGE
            var pso = new PSObject();
            var setItemProperty = this.mocks.Create<ISetItemProperty>();
            setItemProperty
                .Setup(sip => sip.SetItemProperty(pso));

            var node = new LeafNode("name", setItemProperty.Object);

            // ACT
            node.SetItemProperty(pso);
        }

        [Fact]
        public void SetItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var pso = new PSObject();
            var node = new LeafNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.SetItemProperty(pso));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'ISetItemProperty'.", result.Message);
        }

        [Fact]
        public void SetItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var pso = new PSObject();
            var setItemProperty = this.mocks.Create<ISetItemProperty>();
            setItemProperty
                .Setup(sip => sip.SetItemPropertyParameters(pso))
                .Returns(parameters);

            var node = new LeafNode("name", setItemProperty.Object);

            // ACT
            var result = node.SetItemPropertyParameters(pso);

            // ASSERT
            Assert.Same(parameters, result);
        }

        #endregion ISetItemProperty

        #region ICopyItemProperty

        [Fact]
        public void CopyItemProperty_invokes_underlying()
        {
            // ARRANGE
            var sourceNode = new LeafNode("name", new { });
            var pso = new PSObject();
            var copyItemProperty = this.mocks.Create<ICopyItemProperty>();
            copyItemProperty
                .Setup(sip => sip.CopyItemProperty(sourceNode, "sourceProperty", "destinationProperty"));

            var destinationNode = new LeafNode("name", copyItemProperty.Object);

            // ACT
            destinationNode.CopyItemProperty(sourceNode, "sourceProperty", "destinationProperty");
        }

        [Fact]
        public void CopyItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var sourceNode = new LeafNode("name", new { });
            var pso = new PSObject();
            var destinationNode = new LeafNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => destinationNode.CopyItemProperty(sourceNode, "sourceProperty", "destinationProperty"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'ICopyItemProperty'.", result.Message);
        }

        [Fact]
        public void CopyItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var copyItemProperty = this.mocks.Create<ICopyItemProperty>();
            copyItemProperty
                .Setup(sip => sip.CopyItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty"))
                .Returns(parameters);

            var destinationNode = new LeafNode("name", copyItemProperty.Object);

            // ACT
            var result = destinationNode.CopyItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void CopyItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var destinationNode = new LeafNode("name", new { });

            // ACT
            var result = destinationNode.CopyItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty");

            // ASSERT
            Assert.Null(result);
        }

        #endregion ICopyItemProperty

        #region IRemoveItemProperty

        [Fact]
        public void RemoveItemProperty_invokes_underlying()
        {
            // ARRANGE
            var removeItemProperty = this.mocks.Create<IRemoveItemProperty>();
            removeItemProperty
                .Setup(sip => sip.RemoveItemProperty("propertyName"));

            var node = new LeafNode("name", removeItemProperty.Object);

            // ACT
            node.RemoveItemProperty("propertyName");
        }

        [Fact]
        public void RemoveItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new LeafNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RemoveItemProperty("propertyName"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IRemoveItemProperty'.", result.Message);
        }

        [Fact]
        public void RemoveItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var removeItemProperty = this.mocks.Create<IRemoveItemProperty>();
            removeItemProperty
                .Setup(sip => sip.RemoveItemPropertyParameters("propertyName"))
                .Returns(parameters);

            var node = new LeafNode("name", removeItemProperty.Object);

            // ACT
            var result = node.RemoveItemPropertyParameters("propertyName");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void RemoveItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var node = new LeafNode("name", new { });

            // ACT
            var result = node.RemoveItemPropertyParameters("propertyName");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRemoveItemProperty

        #region IMoveItemProperty

        [Fact]
        public void MoveItemProperty_invokes_underlying()
        {
            // ARRANGE
            var sourceNode = new LeafNode("name", new { });
            var pso = new PSObject();
            var moveItemProperty = this.mocks.Create<IMoveItemProperty>();
            moveItemProperty
                .Setup(sip => sip.MoveItemProperty(sourceNode, "sourceProperty", "destinationProperty"));

            var destinationNode = new LeafNode("name", moveItemProperty.Object);

            // ACT
            destinationNode.MoveItemProperty(sourceNode, "sourceProperty", "destinationProperty");
        }

        [Fact]
        public void MoveItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var sourceNode = new LeafNode("name", new { });
            var destinationNode = new LeafNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => destinationNode.MoveItemProperty(sourceNode, "sourceProperty", "destinationProperty"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IMoveItemProperty'.", result.Message);
        }

        [Fact]
        public void MoveItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var sourceNode = new LeafNode("name", new { });
            var parameters = new object();
            var moveItemProperty = this.mocks.Create<IMoveItemProperty>();
            moveItemProperty
                .Setup(sip => sip.MoveItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty"))
                .Returns(parameters);

            var destinationNode = new LeafNode("name", moveItemProperty.Object);

            // ACT
            var result = destinationNode.MoveItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty");

            // ASSERT
            Assert.Same(parameters, parameters);
        }

        [Fact]
        public void MoveItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var sourceNode = new LeafNode("name", new { });
            var destinationNode = new LeafNode("name", new { });

            // ACT
            var result = destinationNode.MoveItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IMoveItemProperty

        #region INewItemProperty

        [Fact]
        public void NewItemProperty_invokes_underlying()
        {
            // ARRANGE
            var newItemProperty = this.mocks.Create<INewItemProperty>();
            newItemProperty
                .Setup(sip => sip.NewItemProperty("propertyName", "propertyTypeName", "value"));

            var node = new LeafNode("name", newItemProperty.Object);

            // ACT
            node.NewItemProperty("propertyName", "propertyTypeName", "value");
        }

        [Fact]
        public void NewItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new LeafNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.NewItemProperty("propertyName", "properytTypeName", "value"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'INewItemProperty'.", result.Message);
        }

        [Fact]
        public void NewItemPropertyParameter_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var newItemProperty = this.mocks.Create<INewItemProperty>();
            newItemProperty
                .Setup(sip => sip.NewItemPropertyParameters("propertyName", "propertyTypeName", "value"))
                .Returns(parameters);

            var node = new LeafNode("name", newItemProperty.Object);

            // ACT
            var result = node.NewItemPropertyParameter("propertyName", "propertyTypeName", "value");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void NewItemPropertyParameter_defaults_to_null()
        {
            // ARRANGE
            var parameters = new object();
            var newItemProperty = this.mocks.Create<INewItemProperty>();
            newItemProperty
                .Setup(sip => sip.NewItemPropertyParameters("propertyName", "propertyTypeName", "value"))
                .Returns(parameters);

            var node = new LeafNode("name", newItemProperty.Object);

            // ACT
            var result = node.NewItemPropertyParameter("propertyName", "propertyTypeName", "value");

            // ASSERT
            Assert.Same(parameters, result);
        }

        #endregion INewItemProperty

        #region IRenameItemProperty

        [Fact]
        public void RanmeItemProperty_invokes_underlying()
        {
            // ARRANGE
            var newItemProperty = this.mocks.Create<IRenameItemProperty>();
            newItemProperty
                .Setup(sip => sip.RenameItemProperty("propertyName", "newPropertyName"));

            var node = new LeafNode("name", newItemProperty.Object);

            // ACT
            node.RenameItemProperty("propertyName", "newPropertyName");
        }

        [Fact]
        public void RanmeItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new LeafNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RenameItemProperty("propertyName", "newPropertyName"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IRenameItemProperty'.", result.Message);
        }

        [Fact]
        public void RanmeItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var newItemProperty = this.mocks.Create<IRenameItemProperty>();
            newItemProperty
                .Setup(sip => sip.RenameItemPropertyParameters("propertyName", "newPropertyName"))
                .Returns(parameters);

            var node = new LeafNode("name", newItemProperty.Object);

            // ACT
            var result = node.RenameItemPropertyParameters("propertyName", "newPropertyName");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void RanmeItemPropertyParameters_default_to_null()
        {
            // ARRANGE
            var node = new LeafNode("name", new { });

            // ACT
            var result = node.RenameItemPropertyParameters("propertyName", "newPropertyName");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRenameItemProperty
    }
}