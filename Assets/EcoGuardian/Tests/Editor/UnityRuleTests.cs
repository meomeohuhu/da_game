using NUnit.Framework;
namespace EcoGuardian.Tests
{
    public class UnityRuleTests
    {
        [Test]public void SeededPlayerActionScenarios(){Assert.AreEqual(25,ScenarioChecks.Run(UnityEngine.Debug.Log,5).sessions);}
        [Test]public void CampaignRules(){Assert.Greater(RuleChecks.Run(UnityEngine.Debug.Log),100);}
    }
}
