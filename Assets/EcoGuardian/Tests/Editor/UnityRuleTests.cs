using NUnit.Framework;
namespace EcoGuardian.Tests
{
    public class UnityRuleTests
    {
        [Test]public void CampaignRules(){Assert.Greater(RuleChecks.Run(UnityEngine.Debug.Log),100);}
    }
}
