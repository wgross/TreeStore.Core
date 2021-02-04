using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
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

        #region IContainerItem

        [Fact]
        public void LeafNode_isnt_container()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.IsContainer;

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void LeafNode_isnt_container_if_underlying_is_IDictionary()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new Dictionary<string, object> { { "Child", new { } } });

            // ACT
            var result = node.IsContainer;

            // ASSERT
            Assert.False(result);
        }

        [Fact(Skip = "IItemCOntainer is recognized as a Container. How to force a Leaf?")]
        public void LeafNode_isnt_container_if_underlying_IItemContainer()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", this.mocks.Create<IItemContainer>().Object);

            // ACT
            var result = node.IsContainer;

            // ASSERT
            Assert.False(result);
        }

        #endregion IContainerItem

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
        public void Invoke_GetItemParameters_creates_empty_parameters()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Empty((RuntimeDefinedParameterDictionary)result);
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
    }
}