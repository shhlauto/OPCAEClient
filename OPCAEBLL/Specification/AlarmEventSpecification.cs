using HLCommon.Core.Specification;
using HLCommon.Core.Specification.Implementation;
using OPCAEModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OPCAEBLL.Specification
{
    public class AlarmEventSpecification : Specification<AlarmEventInfo>
    {
        private Specification<AlarmEventInfo> _spec = new TrueSpecification<AlarmEventInfo>();
        public AlarmEventSpecification(string idStr = "", string AESource = "")
        {
            int id;
            int.TryParse(idStr,out id);
            if (!string.IsNullOrEmpty(idStr))
            {
                _spec &= new DirectSpecification<AlarmEventInfo>(o => o.ID == id);
            }
            if (!string.IsNullOrEmpty(AESource))
            {
                _spec &= new DirectSpecification<AlarmEventInfo>(o => o.AESource == AESource);
            }
        }

        public override Expression<Func<AlarmEventInfo, bool>> SatisfiedBy()
        {
            return _spec.SatisfiedBy();
        }

    }
}
