﻿namespace Selenium.Algorithms.UnitTests.Runs
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Shouldly;
    using System.IO;
    using System.Threading.Tasks;
    using Selenium.Algorithms;
    using System.Drawing;

    public sealed class NestedElementTests
    {
        [Fact]
        public async Task Run_WhenAnActionableElementHasOtherNestedElements_ThenItSuccessfullyFindsTheCorrectActions()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                driver.Manage().Window.Size = new Size(1000, 768);

                try
                {
                    var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenAnActionableElementHasOtherNestedElements_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                    var random = new Random(1);
                    var seleniumEnvironment = new SeleniumEnvironment(
                        driver,
                        fileUri.AbsoluteUri,
                        hasReachedGoalCondition: (driver, _) =>
                        {
                            var target = driver.FindElementByCssSelector(".third");
                            return target.Displayed && target.Enabled;
                        });
                    var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    await rlTrainer.Run(epochs: 2, maximumActions: 15);

                    var initialState = await seleniumEnvironment.GetInitialState();
                    var pathList = await rlTrainer.Walk(initialState, goalCondition: (s, a) => seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    pathList.State.ShouldBe(WalkResultState.GoalReached);
                    pathList.Steps.ShouldNotBeNull();
                    pathList.Steps.ShouldNotBeEmpty();
                    pathList.Steps.Count.ShouldBe(3);
                    pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                    pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                    pathList.Steps[2].Action.ToString().ShouldEndWith("div[data-automation-id='third']");
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }
            }
        }
    }
}