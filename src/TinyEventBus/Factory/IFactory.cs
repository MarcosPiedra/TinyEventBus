using System;
using System.Collections.Generic;
using System.Text;

namespace TinyEventBus.Factory
{
    public interface IFactory<in I1, out O>
    {
        O Get(I1 input1);
    }
    public interface IFactory<in I1, in I2, out O>
    {
        O Get(I1 input1, I2 input2);
    }
    public interface IFactory<in I1, in I2, in I3, out O>
    {
        O Get(I1 input1, I2 input2, I3 input3);
    }
}
