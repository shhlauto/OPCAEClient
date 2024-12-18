﻿using HLCommon.Core.Specification.Contract;
using HLCommon.Core.Specification.Implementation;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace HLCommon.Core.Specification
{
    /// <summary>
    /// A logic Not Specification
    /// </summary>
    /// <typeparam name="T">Type of entity that check this specification</typeparam>
    public sealed class NotSpecification<T> : Specification<T> where T : class
    {
        #region Members

        Expression<Func<T, bool>> _OriginalCriteria;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for NotSpecificaiton
        /// </summary>
        /// <param name="originalSpecification">Original specification</param>
        public NotSpecification(ISpecification<T> originalSpecification)
        {

            if (originalSpecification == (ISpecification<T>)null)
                throw new ArgumentNullException("originalSpecification");

            _OriginalCriteria = originalSpecification.SatisfiedBy();
        }

        /// <summary>
        /// Constructor for NotSpecification
        /// </summary>
        /// <param name="originalSpecification">Original specificaiton</param>
        public NotSpecification(Expression<Func<T, bool>> originalSpecification)
        {
            if (originalSpecification == (Expression<Func<T, bool>>)null)
                throw new ArgumentNullException("originalSpecification");

            _OriginalCriteria = originalSpecification;
        }

        #endregion

        #region Override Specification methods

        /// <summary>
        /// Implementation for method SatidfiedBy
        /// </summary>
        /// <returns></returns>
        public override Expression<Func<T, bool>> SatisfiedBy()
        {

            return Expression.Lambda<Func<T, bool>>(Expression.Not(_OriginalCriteria.Body),
                                                         _OriginalCriteria.Parameters.Single());
        }

        #endregion

    }
}
