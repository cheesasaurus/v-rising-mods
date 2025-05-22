using System.Collections.Generic;

namespace CastleHeartPolice.Models;

// result of checking a rule
public class CheckRuleResult {
    public bool IsViolation = false;
    public List<string> ViolationReasons = new() { };

    public CheckRuleResult AddViolation(string reason) {
        IsViolation = true;
        ViolationReasons.Add(reason);
        return this;
    }

    public static CheckRuleResult Allowed() {
        return new CheckRuleResult();
    }

}
