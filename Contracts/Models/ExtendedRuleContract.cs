using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.Contracts.Models;

internal class ExtendedRuleContract
{



    // Extensions (optional)
    public ExecutionConstraints? Constraints { get; set; }
    public ScoringModule? Scoring { get; set; }
    public AuditModule? Audit { get; set; }
    public DependencyModule? Dependencies { get; set; }
    public TaggingModule? Tags { get; set; }
    public ResultSchemaModule? ResultSchema { get; set; }

}
    

