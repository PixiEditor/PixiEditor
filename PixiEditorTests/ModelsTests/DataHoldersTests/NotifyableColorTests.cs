using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class NotifyableColorTests
    {
        [Fact]
        public void TestThatSetArgbWorks()
        {
            NotifyableColor color = new NotifyableColor();
            color.SetArgb(2,2,2,2);
            Assert.Equal(2, color.A);
            Assert.Equal(2, color.R);
            Assert.Equal(2, color.G);
            Assert.Equal(2, color.B);
        }


        [Theory]
        [InlineData("A", 2)]
        [InlineData("R", 2)]
        [InlineData("G", 2)]
        [InlineData("B", 2)]
        public void TestThatPropertyChangeCalled(string prop, byte value)
        {
            NotifyableColor color = new NotifyableColor(Colors.Black);
            var property = color.GetType().GetProperty(prop);

            Assert.NotNull(property);
            Assert.PropertyChanged(color, prop, () => property.SetValue(color, value));
        }

        [Theory]
        [InlineData("A", 2)]
        [InlineData("R", 2)]
        [InlineData("G", 2)]
        [InlineData("B", 2)]
        public void TestThatEventCalled(string prop, byte value)
        {
            bool eventCalled = false;
            NotifyableColor color = new NotifyableColor(Colors.Black);
            var property = color.GetType().GetProperty(prop);

            color.ColorChanged += (s,e) => eventCalled = true;
            Assert.NotNull(property);

            property.SetValue(color, value);

            Assert.True(eventCalled);
        }
    }
}
