using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ResxFormatterTests.TestFoundation
{
    internal abstract class TheoryDataBase : IEnumerable<object[]>
    {
        public abstract IEnumerator<object[]> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal abstract class TheoryDataBase<T> : TheoryDataBase
    {
        public abstract IEnumerable<T> Create();

        public override IEnumerator<object[]> GetEnumerator()
        {
            return this.Create().Select(o => new object[] { o }).GetEnumerator();
        }
    }

    internal abstract class TheoryDataBase<T1, T2> : TheoryDataBase
    {
        public abstract IEnumerable<(T1, T2)> Create();

        public override IEnumerator<object[]> GetEnumerator()
        {
            return this.Create().Select(o => new object[] { o.Item1, o.Item2 }).GetEnumerator();
        }
    }

    internal abstract class TheoryDataBase<T1, T2, T3> : TheoryDataBase
    {
        public abstract IEnumerable<(T1, T2, T3)> Create();

        public override IEnumerator<object[]> GetEnumerator()
        {
            return this.Create().Select(o => new object[] { o.Item1, o.Item2, o.Item3 }).GetEnumerator();
        }
    }

    internal abstract class TheoryDataBase<T1, T2, T3, T4> : TheoryDataBase
    {
        public abstract IEnumerable<(T1, T2, T3, T4)> Create();

        public override IEnumerator<object[]> GetEnumerator()
        {
            return this.Create().Select(o => new object[] { o.Item1, o.Item2, o.Item3, o.Item4 }).GetEnumerator();
        }
    }

    internal abstract class TheoryDataBase<T1, T2, T3, T4, T5> : TheoryDataBase
    {
        public abstract IEnumerable<(T1, T2, T3, T4, T5)> Create();

        public override IEnumerator<object[]> GetEnumerator()
        {
            return this.Create().Select(o => new object[] { o.Item1, o.Item2, o.Item3, o.Item4, o.Item5 }).GetEnumerator();
        }
    }
}