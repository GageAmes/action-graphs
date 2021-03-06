﻿namespace ActionGraph

open System

module GraphEvents =
    let DefaultEventActions =
        Map.ofSeq(seq{
            yield ("assignFrom",
                function(fromNode : Node, toNode: Node, observingNode: Node, velocity:GraphValue, graph:Graph) ->
                        observingNode.Value <- fromNode.Value
                        ()
            );
            yield ("assignFromToId",
                function(fromNode : Node, toNode: Node, observingNode: Node, velocity:GraphValue, graph:Graph) ->
                        observingNode.Value <- GraphValue(toNode.Id)
                        ()
            );
            yield ("text",
                function(fromNode : Node, toNode: Node, observingNode: Node, velocity:GraphValue, graph:Graph) ->
                        Console.WriteLine(observingNode.Value.ToString())
                        ()
            );
            yield ("increment",
                function(fromNode : Node, toNode: Node, observingNode: Node, velocity:GraphValue, graph:Graph) ->
                        match GraphConversions.collapseGraphLikeToInt(observingNode.Value) with
                        | Some g -> 
                            observingNode.Value <- GraphConversions.assignIntAsGraphLike(g+1)
                        | None -> ()
                        ()
            );
            yield ("zero",
                function(fromNode : Node, toNode: Node, observingNode: Node, velocity:GraphValue, graph:Graph) ->
                        observingNode.Value <- GraphConversions.assignIntAsGraphLike(0)
            );
        })
    let DefaultEventLog =
        {
            Events = []
            EventActions = EventAction(DefaultEventActions)
        }
    let AddEvent(eventLog:EventLog, graphEvent) =
        eventLog.Events <- graphEvent :: eventLog.Events
    let ProcessEvent(eventLog:EventLog, valueNode, toNode, velocity, edgeId, graph) =
        for event in eventLog.Events do
            //if event target matches valueNode+edgeId
            let target = GraphExpressions.ParseTarget(event.EventTarget)
            if(edgeId = target.Target && GraphExpressions.WalkTargetExpression(event.ObserverNode, target) = valueNode) then
                match eventLog.EventActions with
                | EventAction a -> a.[event.Action](valueNode,toNode, event.ObserverNode, velocity, graph)
                    
            ()
        ()
    type EventLog with
        member x.AddEvent(graphEvent:GraphEvent) = AddEvent(x,graphEvent)
        member x.ProcessEvent(valueNode, toNode, velocity, edgeId, graph) = ProcessEvent(x, valueNode, toNode, velocity, edgeId, graph)