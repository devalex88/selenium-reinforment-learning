﻿namespace Selenium.Algorithms.IntegrationTests.Runs
{
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

    public sealed class WaitTests
    {
        [Fact]
        public async Task Run_WhenThereIsADelayedAction_ThenItSuccessfullyWaits()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            using var driver = new ChromeDriver(@".\", chromeOptions);
            driver.Manage().Window.Size = new Size(1000, 768);

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenThereIsADelayedAction_ThenItSuccessfullyWaits)}.html"));
                var random = new Random(1);
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                var seleniumTrainGoal = new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (_1, _2) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    var target = driver.FindElementByCssSelector(".third-panel");
                    return target.Displayed && target.Enabled;
                });
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    fileUri.AbsoluteUri);
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumTrainGoal);

                await rlTrainer.Run(epochs: 4, maximumActions: 50);

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy);
                var pathList = await pathFinder.FindRoute(initialState, goalCondition: seleniumTrainGoal.HasReachedAGoalCondition, maxSteps: 200);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
                pathList.Steps.ShouldNotBeNull();
                pathList.Steps.ShouldNotBeEmpty();
                pathList.Steps.Count.ShouldBe(2);
                pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}