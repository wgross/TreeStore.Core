using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Capabilities;
using TreeStore.Core.Nodes;
using TreeStore.Core.Providers;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test
{
    public class LeafNodeTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ICmdletProvider> providerMock;

        public LeafNodeTest()
        {
            this.providerMock = this.mocks.Create<ICmdletProvider>();
        }

        public void Dispose() => this.mocks.VerifyAll();

        [Fact]
        public void LeafNode_rejects_null_data()
        {
            // ACT & ASSERT
            var node = Assert.Throws<ArgumentNullException>(() => ArrangeLeafNode(this.providerMock.Object, "name", null));
        }

        #region Name

        [Fact]
        public void LeafNode_rejects_null_name()
        {
            // ACT & ASSERT
            var node = Assert.Throws<ArgumentNullException>(() => ArrangeLeafNode(this.providerMock.Object, null, ServiceProvider()));
        }

        [Fact]
        public void LeafNode_provides_name()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, name: "name", ServiceProvider());

            // ACT
            var result = node.Name;

            // ASSERT
            Assert.Equal("name", result);
        }

        #endregion Name

        #region IGetItem

        private class GetItemData : IServiceProvider
        {
            public string Data => "data";

            public object GetService(Type serviceType) => null;
        }

        [Fact]
        public void Invoke_GetItem_creates_PSObject_of_underlying()
        {
            // ARRANGE
            var node = ArrangeLeafNode(this.providerMock.Object, "name", new GetItemData());

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

            var node = ArrangeLeafNode(this.providerMock.Object, "name", ServiceProvider(With<IGetItem>(getItem)));

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_GetItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = ArrangeLeafNode(providerMock.Object, "name", ServiceProvider());

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
                .Setup(gi => gi.GetItem(this.providerMock.Object))
                .Returns(psObject);

            var node = ArrangeLeafNode(this.providerMock.Object, "name", ServiceProvider(With<IGetItem>(getItem)));

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
                .Setup(gi => gi.GetItem(this.providerMock.Object))
                .Returns(psObject);

            var node = ArrangeLeafNode(this.providerMock.Object, "name", ServiceProvider(With<IGetItem>(getItem)));

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
                .Setup(cip => cip.ClearItemProperty(this.providerMock.Object, new[] { "property" }));

            var node = ContainerNode(this.providerMock.Object, "name", sp => sp.AddSingleton(clearItemProperty.Object));

            // ACT
            node.ClearItemProperty(new[] { "property" });
        }

        [Fact]
        public void Invoke_ClearItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = ContainerNode(this.providerMock.Object, "name");

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

            var node = ContainerNode(this.providerMock.Object, "name", With<IClearItemProperty>(clearItemProperty));

            // ACT
            var result = node.ClearItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_ClearItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var node = ContainerNode(this.providerMock.Object, "name");

            // ACT
            var result = node.ClearItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Null(result);
        }

        #endregion IClearItemProperty

        #region IGetItemProperty

        public class GetItemPropertyData : IServiceProvider
        {
            public int Property => 1;

            public string SecondProperty = "text";

            public object GetService(Type serviceType) => null;
        }

        [Fact]
        public void Invoke_GetItemProperty_projects_GetItem()
        {
            // ARRANGE
            var pso = new PSObject();
            pso.Properties.Add(new PSNoteProperty("property", 1));

            var getItemProperty = this.mocks.Create<IGetItemProperty>();
            getItemProperty
                .Setup(gi => gi.GetItemProperty(this.providerMock.Object, new[] { "property" }))
                .Returns(pso);

            var node = LeafNode(this.providerMock.Object, "name", With<IGetItemProperty>(getItemProperty));

            // ACT
            var result = node.GetItemProperty(new[] { "property" });

            // ASSERT
            Assert.Equal(1, result.Properties.Single(p => p.Name.Equals("property")).Value);
        }

        [Fact]
        public void Invoke_GetItemProperty_at_PSObject_without_picklist_returns_complete_object()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", new GetItemPropertyData());

            // ACT
            var result = node.GetItemProperty(null);

            // ASSERT
            Assert.Equal("name", result.Properties.ElementAt(0).Value);
            Assert.Equal(1, result.Properties.ElementAt(1).Value);
            Assert.Equal("text", result.Properties.ElementAt(2).Value);
        }

        [Fact]
        public void Invoke_GetItemProperty_at_PSObject_ignores_unkown_properties()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", new GetItemPropertyData());

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
                .Setup(gip => gip.GetItemProperty(this.providerMock.Object, new[] { "property" }))
                .Returns(new PSObject());

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IGetItemProperty>(getItemProperty)));

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

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IGetItemProperty>(getItemProperty)));

            // ACT
            var result = node.GetItemPropertyParameters(new[] { "property" });

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_GetItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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
                .Setup(sip => sip.SetItemProperty(this.providerMock.Object, pso));

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<ISetItemProperty>(setItemProperty)));

            // ACT
            node.SetItemProperty(pso);
        }

        [Fact]
        public void SetItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var pso = new PSObject();
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<ISetItemProperty>(setItemProperty)));

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
            var sourceNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

            var copyItemProperty = this.mocks.Create<ICopyItemProperty>();
            copyItemProperty
                .Setup(sip => sip.CopyItemProperty(this.providerMock.Object, sourceNode, "sourceProperty", "destinationProperty"));

            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<ICopyItemProperty>(copyItemProperty)));

            // ACT
            destinationNode.CopyItemProperty(sourceNode, "sourceProperty", "destinationProperty");
        }

        [Fact]
        public void CopyItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var sourceNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());
            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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

            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<ICopyItemProperty>(copyItemProperty)));

            // ACT
            var result = destinationNode.CopyItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void CopyItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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
                .Setup(sip => sip.RemoveItemProperty(this.providerMock.Object, "propertyName"));

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IRemoveItemProperty>(removeItemProperty)));

            // ACT
            node.RemoveItemProperty("propertyName");
        }

        [Fact]
        public void RemoveItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IRemoveItemProperty>(removeItemProperty)));

            // ACT
            var result = node.RemoveItemPropertyParameters("propertyName");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void RemoveItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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
            var sourceNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

            var moveItemProperty = this.mocks.Create<IMoveItemProperty>();
            moveItemProperty
                .Setup(sip => sip.MoveItemProperty(this.providerMock.Object, sourceNode, "sourceProperty", "destinationProperty"));

            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IMoveItemProperty>(moveItemProperty)));

            // ACT
            destinationNode.MoveItemProperty(sourceNode, "sourceProperty", "destinationProperty");
        }

        [Fact]
        public void MoveItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var sourceNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());
            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => destinationNode.MoveItemProperty(sourceNode, "sourceProperty", "destinationProperty"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IMoveItemProperty'.", result.Message);
        }

        [Fact]
        public void MoveItemPropertyParameters_invokes_underlying()
        {
            // ARRANGE
            var sourceNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());
            var parameters = new object();
            var moveItemProperty = this.mocks.Create<IMoveItemProperty>();
            moveItemProperty
                .Setup(sip => sip.MoveItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty"))
                .Returns(parameters);

            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IMoveItemProperty>(moveItemProperty)));

            // ACT
            var result = destinationNode.MoveItemPropertyParameters("sourcePath", "sourceProperty", "destinationPath", "destinationProperty");

            // ASSERT
            Assert.Same(parameters, parameters);
        }

        [Fact]
        public void MoveItemPropertyParameters_defaults_to_null()
        {
            // ARRANGE
            var sourceNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());
            var destinationNode = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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
                .Setup(sip => sip.NewItemProperty(this.providerMock.Object, "propertyName", "propertyTypeName", "value"));

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<INewItemProperty>(newItemProperty)));

            // ACT
            node.NewItemProperty("propertyName", "propertyTypeName", "value");
        }

        [Fact]
        public void NewItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<INewItemProperty>(newItemProperty)));

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

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<INewItemProperty>(newItemProperty)));

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
            var renameItemProperty = this.mocks.Create<IRenameItemProperty>();
            renameItemProperty
                .Setup(sip => sip.RenameItemProperty(this.providerMock.Object, "propertyName", "newPropertyName"));

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IRenameItemProperty>(renameItemProperty)));

            // ACT
            node.RenameItemProperty("propertyName", "newPropertyName");
        }

        [Fact]
        public void RanmeItemProperty_defaults_to_exception()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

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
            var renameItemProperty = this.mocks.Create<IRenameItemProperty>();
            renameItemProperty
                .Setup(sip => sip.RenameItemPropertyParameters("propertyName", "newPropertyName"))
                .Returns(parameters);

            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider(With<IRenameItemProperty>(renameItemProperty)));

            // ACT
            var result = node.RenameItemPropertyParameters("propertyName", "newPropertyName");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void RanmeItemPropertyParameters_default_to_null()
        {
            // ARRANGE
            var node = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

            // ACT
            var result = node.RenameItemPropertyParameters("propertyName", "newPropertyName");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRenameItemProperty
    }
}