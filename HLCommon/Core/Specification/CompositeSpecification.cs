﻿using HLCommon.Core.Specification.Contract;
using HLCommon.Core.Specification.Implementation;

namespace HLCommon.Core.Specification
{
    /// <summary>
    /// Base class for composite specifications
    /// </summary>
    /// <typeparam name="T">Type of entity that check this specification</typeparam>
    public abstract class CompositeSpecification<T> : Specification<T> where T : class
    {
        #region Properties

        /// <summary>
        /// Left side specification for this composite element
        /// </summary>
        public abstract ISpecification<T> LeftSideSpecification { get; }

        /// <summary>
        /// Right side specification for this composite element
        /// </summary>
        public abstract ISpecification<T> RightSideSpecification { get; }

        #endregion

    }
}
