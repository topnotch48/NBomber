﻿namespace NBomber.FSharp

open System
open System.Threading.Tasks

open NBomber
open NBomber.Contracts
open NBomber.DomainServices

module Step =    
    open NBomber.Domain.DomainTypes

    let createRequest (name: string, execute: Request -> Task<Response>) = 
        Request({ StepName = name; Execute = execute }) :> IStep  
    
    let createListener (name: string, listeners: IStepListenerChannel) = 
        let ls = listeners :?> StepListenerChannel
        Listener({ StepName = name; Listeners = ls }) :> IStep

    let createPause (duration) = Pause(duration) :> IStep

    let createListenerChannel () = StepListenerChannel() :> IStepListenerChannel

module Assertion =
    open NBomber.Domain.DomainTypes

    let forScenario (assertion: AssertStats -> bool) = Scenario(assertion) :> IAssertion
    let forTestFlow (flowName, assertion: AssertStats -> bool) = TestFlow(flowName, assertion) :> IAssertion
    let forStep (stepName, flowName, assertion: AssertStats -> bool) = Step(stepName, flowName, assertion) :> IAssertion

module Scenario =
    open NBomber.DomainServices.ScenarioRunner

    let create (name: string): Scenario =        
        { ScenarioName = name
          TestInit = None
          TestFlows = Array.empty
          Duration = TimeSpan.FromSeconds(10.0)
          Assertions = Array.empty }

    let withTestInit (initFunc: Request -> Task<Response>) (scenario: Scenario) =
        let step = Step.createRequest(Domain.DomainTypes.Constants.InitId, initFunc)
        { scenario with TestInit = Some(step) }

    let addTestFlow (testFlow: Contracts.TestFlow) (scenario: Scenario) =        
        { scenario with TestFlows = Array.append scenario.TestFlows [|testFlow|] }    

    let withAssertions (assertions: IAssertion list) (scenario: Scenario) =
        { scenario with Assertions = List.toArray(assertions) }

    let withDuration (duration: TimeSpan) (scenario: Scenario) =
        { scenario with Duration = duration }    

    let run (scenario: Scenario) =        
        let config = { IsVerbose = false
                       ShouldSaveReport = true }
        ScenarioRunner.runScenario(scenario, config) |> ignore

    let runInConsole (scenario: Scenario) =
        ScenarioRunner.runInConsole(scenario)

    let runTest (scenario: Scenario) =         
        ScenarioRunner.runTest(scenario)        