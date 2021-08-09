namespace Plotly.NET

open System
open System.IO
//open FSharp.Care.Collections

open GenericChart
open Trace
open Trace3d
open StyleParam
open System.Runtime.InteropServices

// ###########
// Copied from FSharp.Care.Collections to remove dependancies
[<AutoOpen>]
module Seq = 

    /// Splits a sequence of pairs into two sequences
    let unzip (input:seq<_>) =
        let (lstA, lstB) = 
            Seq.foldBack (fun (a,b) (accA, accB) -> 
                a::accA, b::accB) input ([],[])
        (Seq.ofList lstA, Seq.ofList lstB)    

    /// Splits a sequence of triples into three sequences
    let unzip3 (input:seq<_>) =
        let (lstA, lstB, lstC) = 
            Seq.foldBack (fun (a,b,c) (accA, accB, accC) -> 
                a::accA, b::accB, c::accC) input ([],[],[])
        (Seq.ofList lstA, Seq.ofList lstB, Seq.ofList lstC) 


/// Provides a set of static methods for creating charts.
type Chart =

    static member private renderScatterTrace (useWebGL:bool) (style: Trace -> Trace) =
        if useWebGL then
            Trace.initScatterGL style
            |> GenericChart.ofTraceObject
        else
            Trace.initScatter style
            |> GenericChart.ofTraceObject

    /// <summary>Creates a chart that is completely invisible when rendered. The Chart object however is NOT empty! Combining this chart with other charts will have unforseen consequences (it has for example invisible axes that can override other axes if used in Chart.Combine)</summary>
    static member Invisible () =
        let hiddenAxis() = 
            Axis.LinearAxis.init(
                ShowGrid        = false,
                ShowLine        = false,
                ShowTickLabels  = false,
                ZeroLine        = false
            )
        
        let trace = Trace("scatter")
        trace.Remove("type") |> ignore
        GenericChart.ofTraceObject trace
        |> GenericChart.mapLayout ( fun l ->
            l
            |> Layout.AddLinearAxis(AxisId.X 1,hiddenAxis())
            |> Layout.AddLinearAxis(AxisId.Y 1,hiddenAxis())
        )


    /// <summary>Creates a Scatter chart. Scatter charts are the basis of Point, Line, and Bubble Charts in Plotly, and can be customized as such. We also provide abstractions for those: Chart.Line, Chart.Point, Chart.Bubble</summary>
    /// <param name="x">Sets the x coordinates of the plotted data.</param>
    /// <param name="y">Sets the y coordinates of the plotted data.</param>
    /// <param name="mode">Determines the drawing mode for this scatter trace.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="Dash">Sets the Line Dash style</param>
    /// <param name="Width">Sets the Line width</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Scatter(x, y, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol  ,
            [<Optional;DefaultParameterValue(null)>] ?Color         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont      ,
            [<Optional;DefaultParameterValue(null)>] ?Dash          ,
            [<Optional;DefaultParameterValue(null)>] ?Width : float ,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL : bool
        ) = 

        let style = 
            TraceStyle.Scatter(
                X           = x             ,
                Y           = y             ,
                Mode        = mode          , 
                ?StackGroup = StackGroup    , 
                ?Orientation= Orientation   , 
                ?GroupNorm  = GroupNorm
            )               
            >> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            >> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
            >> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            >> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        let useWebGL = defaultArg UseWebGL false

        Chart.renderScatterTrace useWebGL style


    /// <summary>Creates a Scatter chart. Scatter charts are the basis of Point, Line, and Bubble Charts in Plotly, and can be customized as such. We also provide abstractions for those: Chart.Line, Chart.Point, Chart.Bubble</summary>
    /// <param name="xy">Sets the x,y coordinates of the plotted data.</param>
    /// <param name="mode">Determines the drawing mode for this scatter trace.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="Dash">Sets the Line Dash style</param>
    /// <param name="Width">Sets the Line width</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Scatter(xy,mode,
            [<Optional;DefaultParameterValue(null)>] ?Name          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol  ,
            [<Optional;DefaultParameterValue(null)>] ?Color         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont      ,
            [<Optional;DefaultParameterValue(null)>] ?Dash          ,
            [<Optional;DefaultParameterValue(null)>] ?Width         ,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool) = 
        let x,y = Seq.unzip xy 
        Chart.Scatter(x, y, mode,
            ?Name           = Name          ,
            ?Showlegend     = Showlegend    ,
            ?MarkerSymbol   = MarkerSymbol  ,
            ?Color          = Color         ,
            ?Opacity        = Opacity       ,
            ?Labels         = Labels        ,
            ?TextPosition   = TextPosition  ,
            ?TextFont       = TextFont      ,
            ?Dash           = Dash          ,
            ?Width          = Width         ,
            ?StackGroup     = StackGroup    ,
            ?Orientation    = Orientation   ,
            ?GroupNorm      = GroupNorm     ,
            ?UseWebGL       = UseWebGL   
            )


    
    /// <summary>Creates a Point chart, which uses Points in a 2D space to visualize data. </summary>
    /// <param name="x">Sets the x coordinates of the plotted data.</param>
    /// <param name="y">Sets the y coordinates of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Point(x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        // if text position or font is set, then show labels (not only when hovering)
        let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)
        let useWebGL = defaultArg UseWebGL false

        let style = 
            TraceStyle.Scatter(
                X           = x,
                Y           = y, 
                Mode        = changeMode StyleParam.Mode.Markers, 
                ?StackGroup = StackGroup, 
                ?Orientation= Orientation, 
                ?GroupNorm  = GroupNorm)              
            >> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            >> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            >> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        Chart.renderScatterTrace useWebGL style

    /// <summary>Creates a Point chart, which uses Points in a 2D space to visualize data. </summary>
    /// <param name="xy">Sets the x,y coordinates of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Point(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        let x,y = Seq.unzip xy 
        Chart.Point(x, y, 
            ?Name           = Name,
            ?Showlegend     = Showlegend,
            ?MarkerSymbol   = MarkerSymbol,
            ?Color          = Color,
            ?Opacity        = Opacity,
            ?Labels         = Labels,
            ?TextPosition   = TextPosition,
            ?TextFont       = TextFont,
            ?StackGroup     = StackGroup,
            ?Orientation    = Orientation,
            ?GroupNorm      = GroupNorm, 
            ?UseWebGL       = UseWebGL   
            )
        
    
    /// <summary>Creates a Line chart, which uses a Line plotted between the given datums in a 2D space to visualize typically an evolution of Y depending on X.</summary>
    /// <param name="x">Sets the x coordinates of the plotted data.</param>
    /// <param name="y">Sets the y coordinates of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="ShowMarkers">Wether to show markers for the individual data points</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="Dash">Sets the Line Dash style</param>
    /// <param name="Width">Sets the Line width</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Line(x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        // if text position or font is set than show labels (not only when hovering)
        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)


        let style = 
            TraceStyle.Scatter(
                X           = x,
                Y           = y,
                Mode        = changeMode StyleParam.Mode.Lines,
                ?StackGroup = StackGroup, 
                ?Orientation= Orientation, 
                ?GroupNorm  = GroupNorm)          
            >> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
            >> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            >> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            >> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        let useWebGL = defaultArg UseWebGL false

        Chart.renderScatterTrace useWebGL style


    /// <summary>Creates a Line chart, which uses a Line plotted between the given datums in a 2D space to visualize typically an evolution of Y depending on X.</summary>
    /// <param name="xy">Sets the x,y coordinates of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="ShowMarkers">Wether to show markers for the individual data points</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="Dash">Sets the Line Dash style</param>
    /// <param name="Width">Sets the Line width</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Line(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        let x,y = Seq.unzip xy 
        Chart.Line(
            x, y, 
            ?Name           = Name,
            ?ShowMarkers    = ShowMarkers,
            ?Showlegend     = Showlegend,
            ?MarkerSymbol   = MarkerSymbol,
            ?Color          = Color,
            ?Opacity        = Opacity,
            ?Labels         = Labels,
            ?TextPosition   = TextPosition,
            ?TextFont       = TextFont,
            ?Dash           = Dash,
            ?Width          = Width,
            ?StackGroup     = StackGroup,   
            ?Orientation    = Orientation,
            ?GroupNorm      = GroupNorm,  
            ?UseWebGL       = UseWebGL   
            )


    /// <summary>Creates a Spline chart. A spline chart is a line chart in which data points are connected by smoothed curves: this modification is aimed to improve the design of a chart.
    /// Very similar to Line Plots, spline charts are typically used to visualize an evolution of Y depending on X. </summary>
    /// <param name="x">Sets the x coordinates of the plotted data.</param>
    /// <param name="y">Sets the y coordinates of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="ShowMarkers">Wether to show markers for the individual data points</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="Dash">Sets the Line Dash style</param>
    /// <param name="Width">Sets the Line width</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Smoothing">   : Sets the amount of smoothing. "0" corresponds to no smoothing (equivalent to a "linear" shape).  Use values between 0. and 1.3</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Spline(x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Smoothing: float,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 

        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)

        let style = 
            TraceStyle.Scatter(
                X = x,
                Y = y, 
                Mode=changeMode StyleParam.Mode.Lines,
                ?StackGroup = StackGroup, 
                ?Orientation= Orientation, 
                ?GroupNorm  = GroupNorm)      
            >> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            >> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width, Shape=StyleParam.Shape.Spline, ?Smoothing=Smoothing)
            >> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            >> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        let useWebGL = defaultArg UseWebGL false
        Chart.renderScatterTrace useWebGL style


    /// <summary>Creates a Spline chart. A spline chart is a line chart in which data points are connected by smoothed curves: this modification is aimed to improve the design of a chart.
    /// Very similar to Line Plots, spline charts are typically used to visualize an evolution of Y depending on X. </summary>
    /// <param name="xy">Sets the x,y coordinates of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="ShowMarkers">Wether to show markers for the individual data points</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="Dash">Sets the Line Dash style</param>
    /// <param name="Width">Sets the Line width</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Smoothing">   : Sets the amount of smoothing. "0" corresponds to no smoothing (equivalent to a "linear" shape).  Use values between 0. and 1.3</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Spline(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Smoothing,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        let x,y = Seq.unzip xy 
        Chart.Spline(x, y, 
            ?Name           = Name,
            ?ShowMarkers    = ShowMarkers,
            ?Showlegend     = Showlegend,
            ?MarkerSymbol   = MarkerSymbol,
            ?Color          = Color,
            ?Opacity        = Opacity,
            ?Labels         = Labels,
            ?TextPosition   = TextPosition,
            ?TextFont       = TextFont,
            ?Dash           = Dash,
            ?Width          = Width,
            ?Smoothing      = Smoothing,
            ?StackGroup     = StackGroup,
            ?Orientation    = Orientation,
            ?GroupNorm      = GroupNorm,  
            ?UseWebGL       = UseWebGL   
        ) 


    /// <summary>Creates a bubble chart. A bubble chart is a variation of the Point chart, where the data points get an additional scale by being rendered as bubbles of different sizes.</summary>
    /// <param name="x">Sets the x coordinates of the plotted data.</param>
    /// <param name="y">Sets the y coordinates of the plotted data.</param>
    /// <param name="sizes">Sets the bubble size of the plotted data</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Bubble(x, y,sizes:seq<#IConvertible>,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        // if text position or font is set than show labels (not only when hovering)
        let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)
        
        let style = 
            TraceStyle.Scatter(
                X = x,
                Y = y, 
                Mode=changeMode StyleParam.Mode.Markers,
                ?StackGroup = StackGroup, 
                ?Orientation= Orientation, 
                ?GroupNorm  = GroupNorm)                  
            >> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            >> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol, MultiSizes=sizes)
            >> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        let useWebGL = defaultArg UseWebGL false
        Chart.renderScatterTrace useWebGL style

    /// <summary>Creates a bubble chart. A bubble chart is a variation of the Point chart, where the data points get an additional scale by being rendered as bubbles of different sizes.</summary>
    /// <param name="xysizes">Sets the x coordinates, y coordinates, and bubble sizes of the plotted data.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover</param>
    /// <param name="Showlegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="MarkerSymbol">Sets the type of symbol that datums are displayed as</param>
    /// <param name="Color">Sets Line/Marker Color</param>
    /// <param name="Opacity">Sets the Opacity of the trace</param>
    /// <param name="Labels">Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the text font of this trace</param>
    /// <param name="StackGroup">Set several traces (on the same subplot) to the same stackgroup in order to add their y values (or their x values if `Orientation` is Horizontal). Stacking also turns `fill` on by default and sets the default `mode` to "lines" irrespective of point count. ou can only stack on a numeric (linear or log) axis. Traces in a `stackgroup` will only fill to (or be filled to) other traces in the same group. With multiple `stackgroup`s or some traces stacked and some not, if fill-linked traces are not already consecutive, the later ones will be pushed down in the drawing order</param>
    /// <param name="Orientation">Sets the stacking direction. Only relevant when `stackgroup` is used, and only the first `orientation` found in the `stackgroup` will be used.</param>
    /// <param name="GroupNorm">Sets the normalization for the sum of this `stackgroup. Only relevant when `stackgroup` is used, and only the first `groupnorm` found in the `stackgroup` will be used</param>
    /// <param name="UseWebGL">If true, plotly.js will use the WebGL engine to render this chart. use this when you want to render many objects at once.</param>
    static member Bubble(xysizes,[<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?StackGroup    ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation   ,
            [<Optional;DefaultParameterValue(null)>] ?GroupNorm     ,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL   : bool
        ) = 
        let x,y,sizes = Seq.unzip3 xysizes 
        Chart.Bubble(
            x, y,sizes,
            ?Name           = Name,
            ?Showlegend     = Showlegend,
            ?MarkerSymbol   = MarkerSymbol,
            ?Color          = Color,
            ?Opacity        = Opacity,
            ?Labels         = Labels,
            ?TextPosition   = TextPosition,
            ?TextFont       = TextFont,
            ?StackGroup     = StackGroup, 
            ?Orientation    = Orientation,
            ?GroupNorm      = GroupNorm, 
            ?UseWebGL       = UseWebGL   
        )
    
    
    /// Displays a range of data by plotting two Y values per data point, with each Y value being drawn as a line 
    [<Obsolete("Use the constructors with the mandatory mode argument for full functionality")>]
    static member Range(x, y, upper, lower,?Name,?ShowMarkers,?Showlegend,?Color,?RangeColor,?Labels,?TextPosition,?TextFont) =             
        // if text position or font is set than show labels (not only when hovering)
        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
                >> StyleParam.ModeUtils.showMarker (isShowMarker)


        let trace = 
            Trace.initScatter (
                    TraceStyle.Scatter(X = x,Y = y, Mode=changeMode StyleParam.Mode.Markers, ?Fillcolor=Color) )               
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend)
            |> TraceStyle.Line(?Color=Color)
            |> TraceStyle.Marker(?Color=Color)
            |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        let lower = 
            Trace.initScatter (
                    TraceStyle.Scatter(X = x,Y = lower, Mode=StyleParam.Mode.Lines, ?Fillcolor=RangeColor) )               
            |> TraceStyle.TraceInfo(Showlegend=false)
            |> TraceStyle.Line(Width=0.)
            |> TraceStyle.Marker(Color=if RangeColor.IsSome then RangeColor.Value else "rgba(0,0,,0.5)")             

        let upper = 
            Trace.initScatter (
                    TraceStyle.Scatter(X = x,Y = upper, Mode=StyleParam.Mode.Lines, ?Fillcolor=RangeColor, Fill=StyleParam.Fill.ToNext_y) )               
            |> TraceStyle.TraceInfo(Showlegend=false)
            |> TraceStyle.Line(Width=0.)
            |> TraceStyle.Marker(Color=if RangeColor.IsSome then RangeColor.Value else "rgba(0,0,,0.5)")             
 
        GenericChart.MultiChart ([lower;upper;trace],Layout(),Config(), DisplayOptions())

    [<Obsolete("Use the constructors with the mandatory mode argument for full functionality")>]
    /// Displays a range of data by plotting two Y values per data point, with each Y value being drawn as a line 
    static member Range(xy, upper, lower,?Name,?ShowMarkers,?Showlegend,?Color,?RangeColor,?Labels,?TextPosition,?TextFont) =   
        let x,y = Seq.unzip xy
        Chart.Range(x, y, upper, lower, ?Name=Name,?ShowMarkers=ShowMarkers,?Showlegend=Showlegend,?Color=Color,?RangeColor=RangeColor,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont)


    /// Displays a range of data by plotting two Y values per data point, with each Y value being drawn as a line 
    static member Range(x, y, upper, lower,mode,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?RangeColor,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?UpperLabels,
            [<Optional;DefaultParameterValue(null)>] ?LowerLabels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue("lower" )>] ?LowerName: string,
            [<Optional;DefaultParameterValue("upper" )>] ?UpperName: string) =            
            
        let upperName = defaultArg UpperName "upper" 
        let lowerName = defaultArg LowerName "lower" 

        // if text position or font is set than show labels (not only when hovering)
        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
                >> StyleParam.ModeUtils.showMarker (isShowMarker)

        let trace = 
            Trace.initScatter (
                    TraceStyle.Scatter(X = x,Y = y, Mode=mode, ?Fillcolor=Color) )               
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend)
            |> TraceStyle.Line(?Color=Color)
            |> TraceStyle.Marker(?Color=Color)
            |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)

        let lower = 
            Trace.initScatter (
                    TraceStyle.Scatter(X = x,Y = lower, Mode=StyleParam.Mode.Lines, ?Fillcolor=RangeColor) )               
            |> TraceStyle.TraceInfo(?Name = Some lowerName, Showlegend=false)
            |> TraceStyle.Line(Width=0.)
            |> TraceStyle.Marker(Color=if RangeColor.IsSome then RangeColor.Value else "rgba(0,0,0,0.5)")             
            |> TraceStyle.TextLabel(?Text=LowerLabels,?Textposition=TextPosition,?Textfont=TextFont)

        let upper = 
            Trace.initScatter (
                    TraceStyle.Scatter(X = x,Y = upper, Mode=StyleParam.Mode.Lines, ?Fillcolor=RangeColor, Fill=StyleParam.Fill.ToNext_y) )               
            |> TraceStyle.TraceInfo(?Name = Some upperName, Showlegend=false)
            |> TraceStyle.Line(Width=0.)
            |> TraceStyle.Marker(Color=if RangeColor.IsSome then RangeColor.Value else "rgba(0,0,0,0.5)")             
            |> TraceStyle.TextLabel(?Text=UpperLabels,?Textposition=TextPosition,?Textfont=TextFont)

        GenericChart.MultiChart ([lower;upper;trace],Layout(),Config(), DisplayOptions())


    /// Displays a range of data by plotting two Y values per data point, with each Y value being drawn as a line 
    static member Range(xy, upper, lower, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?RangeColor,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?UpperLabels,
            [<Optional;DefaultParameterValue(null)>] ?LowerLabels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?LowerName,
            [<Optional;DefaultParameterValue(null)>] ?UpperName) =  
        let x,y = Seq.unzip xy
        Chart.Range(x, y, upper, lower, mode, ?Name=Name,?ShowMarkers=ShowMarkers,?Showlegend=Showlegend,?Color=Color,?RangeColor=RangeColor,?Labels=Labels,?UpperLabels=UpperLabels,?LowerLabels=LowerLabels,?TextPosition=TextPosition,?TextFont=TextFont,?LowerName=LowerName,?UpperName=UpperName)


    /// Emphasizes the degree of change over time and shows the relationship of the parts to a whole.
    static member Area(x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width) = 
        // if text position or font is set than show labels (not only when hovering)
        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)

        Trace.initScatter (
                TraceStyle.Scatter(X = x,Y = y, Mode=changeMode StyleParam.Mode.Lines,Fill=StyleParam.Fill.ToZero_y) )               
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 


    /// Emphasizes the degree of change over time and shows the relationship of the parts to a whole.
    static member Area(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width) = 
        let x,y = Seq.unzip xy
        Chart.Area(x, y, ?Name=Name,?ShowMarkers=ShowMarkers,?Showlegend=Showlegend,?MarkerSymbol=MarkerSymbol,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Dash=Dash,?Width=Width) 


    /// Emphasizes the degree of change over time and shows the relationship of the parts to a whole.
    static member SplineArea(x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Smoothing) = 
        // if text position or font is set than show labels (not only when hovering)
        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)
  
        Trace.initScatter (
                TraceStyle.Scatter(X = x,Y = y, Mode=changeMode StyleParam.Mode.Lines,Fill=StyleParam.Fill.ToZero_y) )               
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width, Shape=StyleParam.Shape.Spline, ?Smoothing=Smoothing)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 


    /// Emphasizes the degree of change over time and shows the relationship of the parts to a whole.
    static member SplineArea(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Smoothing) = 
        let x,y = Seq.unzip xy
        Chart.SplineArea(x, y, ?Name=Name,?ShowMarkers=ShowMarkers,?Showlegend=Showlegend,?MarkerSymbol=MarkerSymbol,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Dash=Dash,?Width=Width,?Smoothing=Smoothing) 

    /// Emphasizes the degree of change over time and shows the relationship of the parts to a whole.
    static member StackedArea(x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width) = 
        Trace.initScatter (
                TraceStyle.Scatter(X = x,Y = y, Mode=StyleParam.Mode.Lines) )               
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> TraceStyle.SetStackGroup "static"
        |> GenericChart.ofTraceObject 

    /// Emphasizes the degree of change over time and shows the relationship of the parts to a whole.
    static member StackedArea(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width) = 
        let x,y = Seq.unzip xy
        Chart.StackedArea(x, y, ?Name=Name,?Showlegend=Showlegend,?MarkerSymbol=MarkerSymbol,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Dash=Dash,?Width=Width) 

    /// Illustrates comparisons among individual items
    static member Column(keys, values,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) = 
        let marker =
            match Marker with 
            | Some marker -> marker |> Plotly.NET.Marker.style(?Color=Color)
            | Option.None        -> Plotly.NET.Marker.init (?Color=Color)
                    
        Trace.initBar (TraceStyle.Bar(X = keys,Y = values,Marker=marker))
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)        
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject  
            

    /// Illustrates comparisons among individual items
    static member Column(keysvalues,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) = 
        let keys,values = Seq.unzip keysvalues
        Chart.Column(keys, values, ?Name=Name,?Showlegend=Showlegend,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Marker=Marker) 


    /// Displays series of column chart type as stacked columns.
    static member StackedColumn(keys, values,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) =            
        let marker =
            match Marker with 
            | Some marker -> marker |> Plotly.NET.Marker.style(?Color=Color)
            | Option.None        -> Plotly.NET.Marker.init (?Color=Color)

        Trace.initBar (TraceStyle.Bar(X = keys,Y = values,Marker=marker))
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)        
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject  
        //|> GenericChart.setLayout (Layout.init (Layout.style(Barmode=StyleParam.Barmode.Stack)))
        |> GenericChart.setLayout (Layout.init (Barmode=StyleParam.Barmode.Stack))


    /// Displays series of column chart type as stacked columns.
    static member StackedColumn(keysvalues,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) =  
        let keys,values = Seq.unzip keysvalues
        Chart.StackedColumn(keys, values,?Name=Name,?Showlegend=Showlegend,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Marker=Marker) 


    /// Illustrates comparisons among individual items
    static member Bar(keys, values,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) = 
        let marker =
            match Marker with 
            | Some marker -> marker |> Plotly.NET.Marker.style(?Color=Color)
            | Option.None        -> Plotly.NET.Marker.init (?Color=Color)
        Trace.initBar (TraceStyle.Bar(X = values,Y = keys,Marker=marker,Orientation = StyleParam.Orientation.Horizontal))
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)        
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject  


    /// Illustrates comparisons among individual items
    static member Bar(keysvalues,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) = 
        let keys,values = Seq.unzip keysvalues
        Chart.Bar(keys, values, ?Name=Name,?Showlegend=Showlegend,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Marker=Marker) 


    /// Displays series of tcolumn chart type as stacked bars.
    static member StackedBar(keys, values,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) = 
        let marker =
            match Marker with 
            | Some marker -> marker |> Plotly.NET.Marker.style(?Color=Color)
            | Option.None        -> Plotly.NET.Marker.init (?Color=Color)
        Trace.initBar (TraceStyle.Bar(X = values,Y = keys,Marker=marker,Orientation = StyleParam.Orientation.Horizontal))
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)        
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject  
        //|> GenericChart.setLayout (Layout.init (Layout.style(Barmode=StyleParam.Barmode.Stack)))
        |> GenericChart.setLayout (Layout.init (Barmode=StyleParam.Barmode.Stack))


    /// Displays series of tcolumn chart type as stacked bars.
    static member StackedBar(keysvalues,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Marker) = 
        let keys,values = Seq.unzip keysvalues
        Chart.StackedBar(keys, values, ?Name=Name,?Showlegend=Showlegend,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Marker=Marker) 


    /// Displays the distribution of data based on the five number summary: minimum, first quartile, median, third quartile, and maximum.            
    static member BoxPlot
        (
            [<Optional;DefaultParameterValue(null)>] ?x,
            [<Optional;DefaultParameterValue(null)>] ?y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Whiskerwidth,
            [<Optional;DefaultParameterValue(null)>] ?Boxpoints,
            [<Optional;DefaultParameterValue(null)>] ?Boxmean,
            [<Optional;DefaultParameterValue(null)>] ?Jitter,
            [<Optional;DefaultParameterValue(null)>] ?Pointpos,
            [<Optional;DefaultParameterValue(null)>] ?Orientation,
            [<Optional;DefaultParameterValue(null)>] ?Marker,
            [<Optional;DefaultParameterValue(null)>] ?Line,
            [<Optional;DefaultParameterValue(null)>] ?Alignmentgroup,
            [<Optional;DefaultParameterValue(null)>] ?Offsetgroup,
            [<Optional;DefaultParameterValue(null)>] ?Notched,
            [<Optional;DefaultParameterValue(null)>] ?NotchWidth,
            [<Optional;DefaultParameterValue(null)>] ?QuartileMethod
        ) = 
            Trace.initBoxPlot (TraceStyle.BoxPlot(?X=x, ?Y = y,
                                ?Whiskerwidth=Whiskerwidth,?Boxpoints=Boxpoints,
                                ?Boxmean=Boxmean,?Jitter=Jitter,?Pointpos=Pointpos,?Orientation=Orientation,?Fillcolor=Fillcolor,
                                ?Marker=Marker,?Line=Line,?Alignmentgroup=Alignmentgroup,?Offsetgroup=Offsetgroup,?Notched=Notched,?NotchWidth=NotchWidth,?QuartileMethod=QuartileMethod
                                ) )
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)   
            |> TraceStyle.Marker(?Color=Color)
            |> GenericChart.ofTraceObject


    /// Displays the distribution of data based on the five number summary: minimum, first quartile, median, third quartile, and maximum.       
    static member BoxPlot(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Whiskerwidth,
            [<Optional;DefaultParameterValue(null)>] ?Boxpoints,
            [<Optional;DefaultParameterValue(null)>] ?Boxmean,
            [<Optional;DefaultParameterValue(null)>] ?Jitter,
            [<Optional;DefaultParameterValue(null)>] ?Pointpos,
            [<Optional;DefaultParameterValue(null)>] ?Orientation,
            [<Optional;DefaultParameterValue(null)>] ?Marker,
            [<Optional;DefaultParameterValue(null)>] ?Line,
            [<Optional;DefaultParameterValue(null)>] ?Alignmentgroup,
            [<Optional;DefaultParameterValue(null)>] ?Offsetgroup,
            [<Optional;DefaultParameterValue(null)>] ?Notched,
            [<Optional;DefaultParameterValue(null)>] ?NotchWidth,
            [<Optional;DefaultParameterValue(null)>] ?QuartileMethod
            ) = 
        let x,y = Seq.unzip xy
        Chart.BoxPlot(x, y, ?Name=Name,?Showlegend=Showlegend,?Color=Color,?Fillcolor=Fillcolor,?Opacity=Opacity,?Whiskerwidth=Whiskerwidth,?Boxpoints=Boxpoints,?Boxmean=Boxmean,?Jitter=Jitter,?Pointpos=Pointpos,?Orientation=Orientation,
                            ?Marker=Marker,?Line=Line,?Alignmentgroup=Alignmentgroup,?Offsetgroup=Offsetgroup,?Notched=Notched,?NotchWidth=NotchWidth,?QuartileMethod=QuartileMethod) 


    /// Displays the distribution of data based on the five number summary: minimum, first quartile, median, third quartile, and maximum.            
    static member Violin
        (
            [<Optional;DefaultParameterValue(null)>] ?x,
            [<Optional;DefaultParameterValue(null)>] ?y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Points,
            [<Optional;DefaultParameterValue(null)>] ?Jitter,
            [<Optional;DefaultParameterValue(null)>] ?Pointpos,
            [<Optional;DefaultParameterValue(null)>] ?Orientation,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Marker,
            [<Optional;DefaultParameterValue(null)>] ?Line,
            [<Optional;DefaultParameterValue(null)>] ?Alignmentgroup,
            [<Optional;DefaultParameterValue(null)>] ?Offsetgroup,
            [<Optional;DefaultParameterValue(null)>] ?Box,
            [<Optional;DefaultParameterValue(null)>] ?Bandwidth,
            [<Optional;DefaultParameterValue(null)>] ?Meanline,
            [<Optional;DefaultParameterValue(null)>] ?Scalegroup,
            [<Optional;DefaultParameterValue(null)>] ?Scalemode,
            [<Optional;DefaultParameterValue(null)>] ?Side,
            [<Optional;DefaultParameterValue(null)>] ?Span,
            [<Optional;DefaultParameterValue(null)>] ?SpanMode,
            [<Optional;DefaultParameterValue(null)>] ?Uirevision
        ) = 
            Trace.initViolin (TraceStyle.Violin(?X=x, ?Y = y,?Points=Points,
                                ?Jitter=Jitter,?Pointpos=Pointpos,?Orientation=Orientation,?Fillcolor=Fillcolor,
                                ?Width=Width,?Marker=Marker,?Line=Line,?Alignmentgroup=Alignmentgroup,?Offsetgroup=Offsetgroup,?Box=Box,?Bandwidth=Bandwidth,?Meanline=Meanline,
                                ?Scalegroup=Scalegroup,?Scalemode=Scalemode,?Side=Side,?Span=Span,?SpanMode=SpanMode,?Uirevision=Uirevision
                                ) )
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)   
            |> TraceStyle.Marker(?Color=Color)
            |> GenericChart.ofTraceObject


    /// Displays the distribution of data based on the five number summary: minimum, first quartile, median, third quartile, and maximum.       
    static member Violin(xy,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Points,
            [<Optional;DefaultParameterValue(null)>] ?Jitter,
            [<Optional;DefaultParameterValue(null)>] ?Pointpos,
            [<Optional;DefaultParameterValue(null)>] ?Orientation,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Marker,
            [<Optional;DefaultParameterValue(null)>] ?Line,
            [<Optional;DefaultParameterValue(null)>] ?Alignmentgroup,
            [<Optional;DefaultParameterValue(null)>] ?Offsetgroup,
            [<Optional;DefaultParameterValue(null)>] ?Box,
            [<Optional;DefaultParameterValue(null)>] ?Bandwidth,
            [<Optional;DefaultParameterValue(null)>] ?Meanline,
            [<Optional;DefaultParameterValue(null)>] ?Scalegroup,
            [<Optional;DefaultParameterValue(null)>] ?Scalemode,
            [<Optional;DefaultParameterValue(null)>] ?Side,
            [<Optional;DefaultParameterValue(null)>] ?Span,
            [<Optional;DefaultParameterValue(null)>] ?SpanMode,
            [<Optional;DefaultParameterValue(null)>] ?Uirevision        
        ) = 
        let x,y = Seq.unzip xy
        Chart.Violin(x, y, ?Name=Name,?Showlegend=Showlegend,?Color=Color,?Fillcolor=Fillcolor,?Opacity=Opacity,?Points=Points,?Jitter=Jitter,?Pointpos=Pointpos,?Orientation=Orientation,
                        ?Width=Width,?Marker=Marker,?Line=Line,?Alignmentgroup=Alignmentgroup,?Offsetgroup=Offsetgroup,?Box=Box,?Bandwidth=Bandwidth,?Meanline=Meanline,
                        ?Scalegroup=Scalegroup,?Scalemode=Scalemode,?Side=Side,?Span=Span,?SpanMode=SpanMode,?Uirevision=Uirevision
            ) 

    static member private renderHeatmapTrace (useWebGL:bool) (style: Trace -> Trace) =
        if useWebGL then
            Trace.initHeatmapGL style
            |> GenericChart.ofTraceObject
        else
            Trace.initHeatmap style
            |> GenericChart.ofTraceObject

    /// Shows a graphical representation of a 3-dimensional surface by plotting constant z slices, called contours, on a 2-dimensional format.
    /// That is, given a value for z, lines are drawn for connecting the (x,y) coordinates where that z value occurs.
    static member Heatmap(data:seq<#seq<#IConvertible>>,
            [<Optional;DefaultParameterValue(null)>] ?ColNames,
            [<Optional;DefaultParameterValue(null)>] ?RowNames,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Showscale,
            [<Optional;DefaultParameterValue(null)>] ?Xgap,
            [<Optional;DefaultParameterValue(null)>] ?Ygap,
            [<Optional;DefaultParameterValue(null)>] ?zSmooth,
            [<Optional;DefaultParameterValue(null)>] ?Colorbar,
            [<Optional;DefaultParameterValue(false)>]?UseWebGL : bool)
            = 
        let style =
            TraceStyle.Heatmap(Z=data,?X=ColNames, ?Y=RowNames,
                                    ?Xgap=Xgap,?Ygap=Ygap,?Colorscale=Colorscale,?Showscale=Showscale,?zSmooth=zSmooth,?Colorbar=Colorbar)
            >> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)

        let useWebGL = defaultArg UseWebGL false

        Chart.renderHeatmapTrace useWebGL style


    /// Shows a graphical representation of data where the individual values contained in a matrix are represented as colors.
    static member Contour(data:seq<#seq<#IConvertible>>,
            [<Optional;DefaultParameterValue(null)>]  ?X,
            [<Optional;DefaultParameterValue(null)>]  ?Y,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Colorscale,
            [<Optional;DefaultParameterValue(null)>]  ?Showscale,
            [<Optional;DefaultParameterValue(null)>]  ?zSmooth,
            [<Optional;DefaultParameterValue(null)>]  ?Colorbar) = 
        Trace.initContour (TraceStyle.Contour(Z=data,?X=X, ?Y=Y,
                                ?Colorscale=Colorscale,?Showscale=Showscale,?zSmooth=zSmooth,?Colorbar=Colorbar) )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)   
        |> GenericChart.ofTraceObject


    /// Shows how proportions of data, shown as pie-shaped pieces, contribute to the data.
    static member Pie(values,
            [<Optional;DefaultParameterValue(null)>]  ?Labels:seq<'IConvertible>,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?Colors,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Hoverinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Textinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity) =         
        Trace.initPie (TraceStyle.Pie(Values=values,?Labels=Labels,?Textinfo=Textinfo))
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity,?Hoverinfo=Hoverinfo)        
        |> TraceStyle.Marker(?Colors=Colors)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 


    /// Shows how proportions of data, shown as pie-shaped pieces, contribute to the data.
    static member Pie(data:seq<#IConvertible*#IConvertible>,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?Colors,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Hoverinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Textinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity) =         
        let values,labels = Seq.unzip data 
        Chart.Pie(values,Labels=labels,?Name=Name,?Showlegend=Showlegend,?Colors=Colors,?TextPosition=TextPosition,?TextFont=TextFont,?Hoverinfo=Hoverinfo,?Textinfo=Textinfo,?Opacity=Opacity)


    /// Shows how proportions of data, shown as pie-shaped pieces, contribute to the data as a whole.
    static member Doughnut(values,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?Colors,
            [<Optional;DefaultParameterValue(null)>]  ?Hole,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Hoverinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Textinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity) =         
        let hole' = if Hole.IsSome then Hole.Value else 0.4
        Trace.initPie (TraceStyle.Pie(Values=values,?Labels=Labels,?Textinfo=Textinfo,Hole=hole'))
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity,?Hoverinfo=Hoverinfo)        
        |> TraceStyle.Marker(?Colors=Colors)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 


    /// Shows how proportions of data, shown as pie-shaped pieces, contribute to the data as a whole.
    static member Doughnut(data:seq<#IConvertible*#IConvertible>,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?Colors,
            [<Optional;DefaultParameterValue(null)>]  ?Hole,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Hoverinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Textinfo,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity) =         
        let values,labels = Seq.unzip data 
        Chart.Doughnut(values,Labels=labels,?Name=Name,?Showlegend=Showlegend,?Colors=Colors,?Hole=Hole,?TextPosition=TextPosition,?TextFont=TextFont,?Hoverinfo=Hoverinfo,?Textinfo=Textinfo,?Opacity=Opacity)


    /// Uses points, line or both depending on the mode to represent data points in a polar chart
    static member ScatterPolar(r, theta, mode,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Dash,
            [<Optional;DefaultParameterValue(null)>]  ?Width) = 

        Trace.initScatterPolar (
            TraceStyle.ScatterPolar(
                R       = r,
                Theta   = theta, 
                Mode    = mode
            ) 
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 



     /// Uses points, line or both depending on the mode to represent data points in a polar chart
    static member ScatterPolar
        (
            rtheta, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width
        ) = 

            let r,t = Seq.unzip rtheta

            Chart.ScatterPolar(
                r, t, mode,
                ?Name=Name,
                ?Showlegend=Showlegend,
                ?MarkerSymbol=MarkerSymbol,
                ?Color=Color,
                ?Opacity=Opacity,
                ?Labels=Labels,
                ?TextPosition=TextPosition,
                ?TextFont=TextFont,
                ?Dash=Dash,
                ?Width=Width
            )

    /// 
    static member PointPolar
        (
            r, theta,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont
        ) = 

            let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)
            
            Trace.initScatterPolar (
                TraceStyle.ScatterPolar(
                    R       = r,
                    Theta   = theta, 
                    Mode    = changeMode StyleParam.Mode.Markers
                ) 
            )
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
            |> GenericChart.ofTraceObject 

    /// 
    static member PointPolar
        (
            rtheta,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont
        ) = 
            let r,t = Seq.unzip rtheta

            Chart.PointPolar(
                r, t,
                ?Name           = Name,
                ?Showlegend     = Showlegend,
                ?MarkerSymbol   = MarkerSymbol,
                ?Color          = Color,
                ?Opacity        = Opacity,
                ?Labels         = Labels,
                ?TextPosition   = TextPosition,
                ?TextFont       = TextFont
            )
            
    ///
    static member LinePolar 
        (
            r, theta,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Dash,
            [<Optional;DefaultParameterValue(null)>]  ?Width
        ) =
            let changeMode = 
                let isShowMarker =
                    match ShowMarkers with
                    | Some isShow -> isShow
                    | Option.None        -> false
                StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
                >> StyleParam.ModeUtils.showMarker (isShowMarker)

            Trace.initScatterPolar (
                TraceStyle.ScatterPolar(
                    R       = r,
                    Theta   = theta, 
                    Mode    = changeMode StyleParam.Mode.Lines
                ) 
            )
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
            |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
            |> GenericChart.ofTraceObject 

    ///
    static member LinePolar 
        (
            rtheta,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Dash,
            [<Optional;DefaultParameterValue(null)>]  ?Width
        ) =
             let r,t = Seq.unzip rtheta

             Chart.LinePolar(
                r, t,
                ?Name           = Name,
                ?Showlegend     = Showlegend,
                ?ShowMarkers    = ShowMarkers,
                ?MarkerSymbol   = MarkerSymbol,
                ?Color          = Color,
                ?Opacity        = Opacity,
                ?Labels         = Labels,
                ?TextPosition   = TextPosition,
                ?TextFont       = TextFont,
                ?Dash           = Dash,
                ?Width          = Width
             )

    ///
    static member SplinePolar 
        (
            r, theta,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Smoothing,
            [<Optional;DefaultParameterValue(null)>]  ?Dash,
            [<Optional;DefaultParameterValue(null)>]  ?Width
        ) =
            let changeMode = 
                let isShowMarker =
                    match ShowMarkers with
                    | Some isShow -> isShow
                    | Option.None        -> false
                StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
                >> StyleParam.ModeUtils.showMarker (isShowMarker)

            Trace.initScatterPolar (
                TraceStyle.ScatterPolar(
                    R       = r,
                    Theta   = theta, 
                    Mode    = changeMode StyleParam.Mode.Lines
                ) 
            )
            |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
            |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width, Shape=StyleParam.Shape.Spline, ?Smoothing=Smoothing)
            |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
            |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
            |> GenericChart.ofTraceObject 

    ///
    static member SplinePolar 
        (
            rtheta,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?ShowMarkers,
            [<Optional;DefaultParameterValue(null)>]  ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Labels,
            [<Optional;DefaultParameterValue(null)>]  ?TextPosition,
            [<Optional;DefaultParameterValue(null)>]  ?TextFont,
            [<Optional;DefaultParameterValue(null)>]  ?Smoothing,
            [<Optional;DefaultParameterValue(null)>]  ?Dash,
            [<Optional;DefaultParameterValue(null)>]  ?Width
        ) =
             let r,t = Seq.unzip rtheta

             Chart.SplinePolar(
                r, t,
                ?Name           = Name,
                ?Showlegend     = Showlegend,
                ?ShowMarkers    = ShowMarkers,
                ?MarkerSymbol   = MarkerSymbol,
                ?Color          = Color,
                ?Opacity        = Opacity,
                ?Labels         = Labels,
                ?TextPosition   = TextPosition,
                ?TextFont       = TextFont,
                ?Smoothing      = Smoothing,
                ?Dash           = Dash,
                ?Width          = Width
             )


    //static member WindRose(r, t,
    //        [<Optional;DefaultParameterValue(null)>] ?Name,
    //        [<Optional;DefaultParameterValue(null)>] ?Showlegend,
    //        [<Optional;DefaultParameterValue(null)>] ?Color,
    //        [<Optional;DefaultParameterValue(null)>] ?Opacity,
    //        [<Optional;DefaultParameterValue(null)>] ?Labels,
    //        [<Optional;DefaultParameterValue(null)>] ?TextPosition,
    //        [<Optional;DefaultParameterValue(null)>] ?TextFont,
    //        [<Optional;DefaultParameterValue(null)>] ?Dash,
    //        [<Optional;DefaultParameterValue(null)>] ?Width) = 
    //    Trace.initWindRose (
    //            TraceStyle.Scatter(R = r,T = t) )               
    //    |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
    //    |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
    //    |> TraceStyle.Marker(?Color=Color)
    //    |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
    //    |> GenericChart.ofTraceObject 

     /// Computes a histogram with auto-determined the bin size.
    static member Histogram(data,
            [<Optional;DefaultParameterValue(null)>]  ?Orientation,
            [<Optional;DefaultParameterValue(null)>]  ?Name,
            [<Optional;DefaultParameterValue(null)>]  ?Showlegend,
            [<Optional;DefaultParameterValue(null)>]  ?Opacity,
            [<Optional;DefaultParameterValue(null)>]  ?Color,
            [<Optional;DefaultParameterValue(null)>]  ?HistNorm,
            [<Optional;DefaultParameterValue(null)>]  ?HistFunc,
            [<Optional;DefaultParameterValue(null)>]  ?nBinsx,
            [<Optional;DefaultParameterValue(null)>]  ?nBinsy,
            [<Optional;DefaultParameterValue(null)>]  ?Xbins,
            [<Optional;DefaultParameterValue(null)>]  ?Ybins,
            // TODO
            [<Optional;DefaultParameterValue(null)>]  ?xError,
            [<Optional;DefaultParameterValue(null)>]  ?yError) =         
        Trace.initHistogram (
            TraceStyle.Histogram (X=data,?Orientation=Orientation,?HistNorm=HistNorm,?HistFunc=HistFunc,
                                    ?nBinsx=nBinsx,?nBinsy=nBinsy,?xBins=Xbins,?yBins=Ybins)
                             )
        |> TraceStyle.Marker(?Color=Color)
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)   
        
        |> GenericChart.ofTraceObject

     /// Computes the bi-dimensional histogram of two data samples and auto-determines the bin size.
    static member Histogram2d(x,y,
            [<Optional;DefaultParameterValue(null)>] ?Z,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Showscale,
            [<Optional;DefaultParameterValue(null)>] ?zSmooth,
            [<Optional;DefaultParameterValue(null)>] ?Colorbar,
            [<Optional;DefaultParameterValue(null)>] ?zAuto,
            [<Optional;DefaultParameterValue(null)>] ?zMin,
            [<Optional;DefaultParameterValue(null)>] ?zMax,
            [<Optional;DefaultParameterValue(null)>] ?nBinsx,
            [<Optional;DefaultParameterValue(null)>] ?nBinsy,
            [<Optional;DefaultParameterValue(null)>] ?xBins,
            [<Optional;DefaultParameterValue(null)>] ?yBins,
            [<Optional;DefaultParameterValue(null)>] ?HistNorm,
            [<Optional;DefaultParameterValue(null)>] ?HistFunc) =         
        Trace.initHistogram2d (
            TraceStyle.Histogram2d (
                                X=x,
                                Y=y,
                                ?Z=Z,
                                ?Colorscale=Colorscale,
                                ?Showscale=Showscale,
                                ?zSmooth=zSmooth,
                                ?Colorbar=Colorbar,
                                ?zAuto=zAuto,
                                ?zMin=zMin,
                                ?zMax=zMax,
                                ?nBinsx=nBinsx,
                                ?nBinsy=nBinsy,
                                ?xBins=xBins,
                                ?yBins=yBins,
                                ?HistNorm=HistNorm,
                                ?HistFunc=HistFunc ) )
      
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)   
        |> GenericChart.ofTraceObject
 

// //    /// Computes the bi-dimensional histogram of two data samples and auto-determines the bin size.
// //    static member Histogram2d(xy,?Name,?HistNorm,?HistFunc,?Colorscale,?Showscale,?zSmooth,?Colorbar,?zAuto,?zMin,?zMax,?nBinsx,?nBinsy,?Xbins,?Ybins) =         
// //        let x,y = Seq.unzip xy
// //        Chart.Histogram2d(x,y,?Name=Name,?HistNorm=HistNorm,?HistFunc=HistFunc,?Colorscale=Colorscale,
// //            ?Showscale=Showscale,?Colorbar=Colorbar,?zSmooth=zSmooth,?zAuto=zAuto,?zMin=zMin,?zMax=zMax,
// //            ?nBinsx=nBinsx,?nBinsy=nBinsy,?Xbins=Xbins,?Ybins=Ybins
// //            )

     /// Computes the bi-dimensional histogram of two data samples and auto-determines the bin size.
    static member Histogram2dContour(x,y,
            [<Optional;DefaultParameterValue(null)>] ?Z,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Showscale,
            [<Optional;DefaultParameterValue(null)>] ?Line,
            [<Optional;DefaultParameterValue(null)>] ?zSmooth,
            [<Optional;DefaultParameterValue(null)>] ?Colorbar,
            [<Optional;DefaultParameterValue(null)>] ?zAuto,
            [<Optional;DefaultParameterValue(null)>] ?zMin,
            [<Optional;DefaultParameterValue(null)>] ?zMax,
            [<Optional;DefaultParameterValue(null)>] ?nBinsx,
            [<Optional;DefaultParameterValue(null)>] ?nBinsy,
            [<Optional;DefaultParameterValue(null)>] ?xBins,
            [<Optional;DefaultParameterValue(null)>] ?yBins,
            [<Optional;DefaultParameterValue(null)>] ?HistNorm,
            [<Optional;DefaultParameterValue(null)>] ?HistFunc) =         
        Trace.initHistogram2dContour (
            TraceStyle.Histogram2dContour (X=x, Y=y,? Z=Z,?Line=Line,
                    ?Colorscale=Colorscale,
                    ?Showscale=Showscale,
                    ?zSmooth=zSmooth,
                    ?Colorbar=Colorbar,
                    ?zAuto=zAuto,
                    ?zMin=zMin,
                    ?zMax=zMax,
                    ?nBinsx=nBinsx,
                    ?nBinsy=nBinsy,
                    ?xBins=xBins,
                    ?yBins=yBins,
                    ?HistNorm=HistNorm,
                    ?HistFunc=HistFunc                                
                                ) )
        //|> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)   
        |> GenericChart.ofTraceObject


     /// Computes the parallel coordinates plot
    static member ParallelCoord(dims:seq<'key*#seq<'values>>,
            [<Optional;DefaultParameterValue(null)>] ?Range,
            [<Optional;DefaultParameterValue(null)>] ?Constraintrange,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Domain,
            [<Optional;DefaultParameterValue(null)>] ?Labelfont,
            [<Optional;DefaultParameterValue(null)>] ?Tickfont,
            [<Optional;DefaultParameterValue(null)>] ?Rangefont) =
        let dims' = 
            dims |> Seq.map (fun (k,vals) -> 
                Dimensions.init(vals)
                |> Dimensions.style(vals,?Range=Range,?Constraintrange=Constraintrange,Label=k)
                )
        Trace.initParallelCoord (
            TraceStyle.ParallelCoord (Dimensions=dims',?Domain=Domain,?Labelfont=Labelfont,?Tickfont=Tickfont,?Rangefont=Rangefont)             
            )
        |> TraceStyle.Line(?Width=Width,?Color=Color,?Dash=Dash,?Colorscale=Colorscale)
        |> GenericChart.ofTraceObject


     /// Computes the parallel coordinates plot
    static member ParallelCoord(dims:seq<Dimensions>,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Domain,
            [<Optional;DefaultParameterValue(null)>] ?Labelfont,
            [<Optional;DefaultParameterValue(null)>] ?Tickfont,
            [<Optional;DefaultParameterValue(null)>] ?Rangefont) =
        Trace.initParallelCoord (
            TraceStyle.ParallelCoord (Dimensions=dims,?Domain=Domain,?Labelfont=Labelfont,?Tickfont=Tickfont,?Rangefont=Rangefont)             
            )
        |> TraceStyle.Line(?Width=Width,?Color=Color,?Dash=Dash,?Colorscale=Colorscale)
        |> GenericChart.ofTraceObject

    ///Parallel categories diagram for multidimensional categorical data.
    static member ParallelCategories(dims:seq<'key*#seq<'values>>,
            [<Optional;DefaultParameterValue(null)>] ?Range,
            [<Optional;DefaultParameterValue(null)>] ?Constraintrange,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Domain,
            [<Optional;DefaultParameterValue(null)>] ?Labelfont,
            [<Optional;DefaultParameterValue(null)>] ?Tickfont,
            [<Optional;DefaultParameterValue(null)>] ?Rangefont) =
        let dims' = 
            dims |> Seq.map (fun (k,vals) -> 
                Dimensions.init(vals)
                |> Dimensions.style(vals,?Range=Range,?Constraintrange=Constraintrange,Label=k)
                )
        Trace.initParallelCategories (
            TraceStyle.ParallelCategories(Dimensions=dims',?Domain=Domain,?Labelfont=Labelfont,?Tickfont=Tickfont,?Rangefont=Rangefont)
        )
        |> TraceStyle.Line(?Width=Width,?Color=Color,?Dash=Dash,?Colorscale=Colorscale)
        |> GenericChart.ofTraceObject

    ///
    static member ParallelCategories(dims:seq<Dimensions>,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Domain,
            [<Optional;DefaultParameterValue(null)>] ?Labelfont,
            [<Optional;DefaultParameterValue(null)>] ?Tickfont,
            [<Optional;DefaultParameterValue(null)>] ?Rangefont) =
        Trace.initParallelCategories (
            TraceStyle.ParallelCategories (Dimensions=dims,?Domain=Domain,?Color=Color,?Labelfont=Labelfont,?Tickfont=Tickfont,?Rangefont=Rangefont)             
            )
        |> TraceStyle.Line(?Width=Width,?Dash=Dash,?Colorscale=Colorscale)
        |> GenericChart.ofTraceObject

    /// Computes the choropleth map plot
    static member ChoroplethMap(locations,z,
            [<Optional;DefaultParameterValue(null)>] ?Text,
            [<Optional;DefaultParameterValue(null)>] ?Locationmode,
            [<Optional;DefaultParameterValue(null)>] ?Autocolorscale,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Colorbar,
            [<Optional;DefaultParameterValue(null)>] ?Marker,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string,
            [<Optional;DefaultParameterValue(null)>] ?Zmin,
            [<Optional;DefaultParameterValue(null)>] ?Zmax) =
        Trace.initChoroplethMap (
            TraceStyle.ChoroplethMap (Locations=locations,Z=z,?Text=Text,?Locationmode=Locationmode,?Autocolorscale=Autocolorscale,
                ?Colorscale=Colorscale,?Colorbar=Colorbar,?Marker=Marker,?Zmin=Zmin,?Zmax=Zmax,?GeoJson=GeoJson,?FeatureIdKey=FeatureIdKey)              
            )
        |> GenericChart.ofTraceObject        

     /// Computes the parallel coordinates plot
    static member Splom(dims:seq<'key*#seq<'values>>,
            [<Optional;DefaultParameterValue(null)>] ?Range,
            [<Optional;DefaultParameterValue(null)>] ?Constraintrange,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            ?Domain,?Labelfont,?Tickfont,?Rangefont) =
        let dims' = 
            dims |> Seq.map (fun (k,vals) -> 
                Dimensions.init(vals)
                |> Dimensions.style(vals,?Range=Range,?Constraintrange=Constraintrange,Label=k)
                )
        Trace.initSplom (
            TraceStyle.Splom (Dimensions=dims')             
            )
        |> TraceStyle.Line(?Width=Width,?Color=Color,?Dash=Dash,?Colorscale=Colorscale)
        |> GenericChart.ofTraceObject


     /// Computes the Splom plot
    static member Splom(dims:seq<Dimensions>,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Width,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Domain,
            [<Optional;DefaultParameterValue(null)>] ?Labelfont,
            [<Optional;DefaultParameterValue(null)>] ?Tickfont,
            [<Optional;DefaultParameterValue(null)>] ?Rangefont) =
        Trace.initSplom (
            TraceStyle.Splom (Dimensions=dims)             
            )
        |> TraceStyle.Line(?Width=Width,?Color=Color,?Dash=Dash,?Colorscale=Colorscale)
        |> GenericChart.ofTraceObject

    // ---------------------------------------------------------------------------------------------------------------------------------------------------
    // 3d - Chart --->

    /// Uses points, line or both depending on the mode to represent 3d-data points
    static member Scatter3d(x, y, z, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width) = 
        Trace3d.initScatter3d (Trace3dStyle.Scatter3d(X = x,Y = y,Z=z, Mode=mode) )              
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 
          
    
    /// Uses points, line or both depending on the mode to represent 3d-data points
    static member Scatter3d(xyz, mode, 
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol,
            [<Optional;DefaultParameterValue(null)>] ?Color,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Labels,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition,
            [<Optional;DefaultParameterValue(null)>] ?TextFont,
            [<Optional;DefaultParameterValue(null)>] ?Dash,
            [<Optional;DefaultParameterValue(null)>] ?Width) = 
        let x,y,z = Seq.unzip3 xyz
        Chart.Scatter3d(x, y, z, mode, ?Name=Name,?Showlegend=Showlegend,?MarkerSymbol=MarkerSymbol,?Color=Color,?Opacity=Opacity,?Labels=Labels,?TextPosition=TextPosition,?TextFont=TextFont,?Dash=Dash,?Width=Width) 

    /// Uses points, line or both depending on the mode to represent 3d-data points
    static member Surface(data:seq<#seq<#IConvertible>>,
            [<Optional;DefaultParameterValue(null)>] ?X,
            [<Optional;DefaultParameterValue(null)>] ?Y,
            [<Optional;DefaultParameterValue(null)>] ?Name,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend,
            [<Optional;DefaultParameterValue(null)>] ?Opacity,
            [<Optional;DefaultParameterValue(null)>] ?Contours,
            [<Optional;DefaultParameterValue(null)>] ?Colorscale,
            [<Optional;DefaultParameterValue(null)>] ?Showscale,
            [<Optional;DefaultParameterValue(null)>] ?Colorbar) = 
        Trace3d.initSurface (
            Trace3dStyle.Surface (Z=data,?X=X, ?Y=Y,?Contours=Contours,
                                ?Colorscale=Colorscale,?Showscale=Showscale,?Colorbar=Colorbar ) )              
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        //|> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 


    /// Uses points, line or both depending on the mode to represent 3d-data points
    static member Mesh3d(x, y, z, mode,
            ?Name,
            ?Showlegend,
            ?MarkerSymbol,
            ?Color,
            ?Opacity,
            ?Labels,
            ?TextPosition,
            ?TextFont,
            ?Dash,
            ?Width) = 
        Trace3d.initMesh3d (Trace3dStyle.Mesh3d(X = x,Y = y,Z=z) )              
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 

    /// creates table out of header sequence and row sequences
    static member Table(headerValues, cellValues, 
            [<Optional;DefaultParameterValue(null)>] ?AlignHeader, 
            [<Optional;DefaultParameterValue(null)>] ?AlignCells,
            [<Optional;DefaultParameterValue(null)>] ?ColumnWidth, 
            [<Optional;DefaultParameterValue(null)>] ?ColumnOrder, 
            [<Optional;DefaultParameterValue(null)>] ?ColorHeader, 
            [<Optional;DefaultParameterValue(null)>] ?ColorCells, 
            [<Optional;DefaultParameterValue(null)>] ?FontHeader, 
            [<Optional;DefaultParameterValue(null)>] ?FontCells, 
            [<Optional;DefaultParameterValue(null)>] ?HeightHeader, 
            [<Optional;DefaultParameterValue(null)>] ?HeightCells, 
            [<Optional;DefaultParameterValue(null)>] ?LineHeader, 
            [<Optional;DefaultParameterValue(null)>] ?LineCells) = 
        Trace.initTable (

                let CellFilling =
                    match ColorCells with 
                    | Some color  -> Some (CellColor.init (?Color=ColorCells))
                    | Option.None -> Option.None

                let HeaderFilling =
                    match ColorHeader with 
                    | Some color   -> Some (CellColor.init (?Color=ColorHeader))
                    | Option.None  -> Option.None
                              
                TraceStyle.Table (
                    header = TableHeader.init (headerValues|> Seq.map seq, ?Align=AlignHeader, ?Fill=HeaderFilling, ?Font=FontHeader, ?Height=HeightHeader, ?Line=LineHeader),
                    cells  = TableCells.init(cellValues |> Seq.transpose, ?Align=AlignCells, ?Fill=CellFilling, ?Font=FontCells, ?Height=HeightCells, ?Line=LineCells),  
                    ?ColumnWidth = ColumnWidth,
                    ?ColumnOrder = ColumnOrder
                    )
                )
        |> GenericChart.ofTraceObject 

    /// Creates a sunburst chart. Visualize hierarchical data spanning outward radially from root to leaves.
    /// Applies the styles of sundburst plot to TraceObjects 
    ///
    /// Parameters:
    ///
    /// labels: Sets the labels of each of the sectors.
    ///
    /// parents: Sets the parent sectors for each of the sectors. Empty string items '' are understood to reference the root node in the hierarchy. If `ids` is filled, `parents` items are understood to be "ids" themselves. When `ids` is not set, plotly attempts to find matching items in `labels`, but beware they must be unique.
    ///
    /// Ids: Assigns id labels to each datum. These ids for object constancy of data points during animation.
    ///
    /// Values: Sets the values associated with each of the sectors. Use with `branchvalues` to determine how the values are summed.
    ///
    /// Text: Sets text elements associated with each sector. If trace `textinfo` contains a "text" flag, these elements will be seen on the chart. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// Branchvalues: Determines how the items in `values` are summed. When set to "total", items in `values` are taken to be value of all its descendants. When set to "remainder", items in `values` corresponding to the root and the branches sectors are taken to be the extra part not part of the sum of the values at their leaves.
    ///
    /// Level: Sets the level from which this trace hierarchy is rendered. Set `level` to `''` to start from the root node in the hierarchy. Must be an "id" if `ids` is filled in, otherwise plotly attempts to find a matching item in `labels`.
    ///
    /// Maxdepth: Sets the number of rendered sectors from any given `level`. Set `maxdepth` to "-1" to render all the levels in the hierarchy.
    ///
    /// Colorbar: Sets the Colorbar for the chart
    ///
    ///Colors: Sets the color of each sector of this trace. If not specified, the default trace color set is used to pick the sector colors.
    static member Sunburst(labels,parents,
        [<Optional;DefaultParameterValue(null)>]?Ids,
        [<Optional;DefaultParameterValue(null)>]?Values             ,
        [<Optional;DefaultParameterValue(null)>]?Text               ,
        [<Optional;DefaultParameterValue(null)>]?Branchvalues       ,
        [<Optional;DefaultParameterValue(null)>]?Level              ,
        [<Optional;DefaultParameterValue(null)>]?Maxdepth           ,
        [<Optional;DefaultParameterValue(null)>]?Colors: seq<string>,
        [<Optional;DefaultParameterValue(null)>]?Colorbar:Colorbar
        ) =
        Trace.initSunburst(
            TraceStyle.Sunburst(
                labels          = labels,
                parents         = parents,
                ?Ids            = Ids,
                ?Values         = Values,
                ?Text           = Text,
                ?Branchvalues   = Branchvalues,
                ?Level          = Level,
                ?Maxdepth       = Maxdepth
            )
        )
        |> TraceStyle.Marker(?Colors=Colors,?Colorbar=Colorbar)
        |> GenericChart.ofTraceObject


    /// Creates a treemap chart. Treemap charts visualize hierarchical data using nested rectangles. Same as Sunburst the hierarchy is defined by labels and parents attributes. Click on one sector to zoom in/out, which also displays a pathbar in the upper-left corner of your treemap. To zoom out you can use the path bar as well.
    ///
    /// Parameters:
    ///
    /// labels: Sets the labels of each of the sectors.
    ///
    /// parents: Sets the parent sectors for each of the sectors. Empty string items '' are understood to reference the root node in the hierarchy. If `ids` is filled, `parents` items are understood to be "ids" themselves. When `ids` is not set, plotly attempts to find matching items in `labels`, but beware they must be unique.
    ///
    /// Ids: Assigns id labels to each datum. These ids for object constancy of data points during animation.
    ///
    /// Values: Sets the values associated with each of the sectors. Use with `branchvalues` to determine how the values are summed.
    ///
    /// Text: Sets text elements associated with each sector. If trace `textinfo` contains a "text" flag, these elements will be seen on the chart. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// Branchvalues: Determines how the items in `values` are summed. When set to "total", items in `values` are taken to be value of all its descendants. When set to "remainder", items in `values` corresponding to the root and the branches sectors are taken to be the extra part not part of the sum of the values at their leaves.
    ///
    /// Level: Sets the level from which this trace hierarchy is rendered. Set `level` to `''` to start from the root node in the hierarchy. Must be an "id" if `ids` is filled in, otherwise plotly attempts to find a matching item in `labels`.
    ///
    /// Maxdepth: Sets the number of rendered sectors from any given `level`. Set `maxdepth` to "-1" to render all the levels in the hierarchy.
    ///
    /// Colorbar: Sets the Colorbar for the chart
    ///
    ///Colors: Sets the color of each sector of this trace. If not specified, the default trace color set is used to pick the sector colors.
    static member Treemap(labels,parents,
        [<Optional;DefaultParameterValue(null)>]?Ids,
        [<Optional;DefaultParameterValue(null)>]?Values             ,
        [<Optional;DefaultParameterValue(null)>]?Text               ,
        [<Optional;DefaultParameterValue(null)>]?Branchvalues       ,
        [<Optional;DefaultParameterValue(null)>]?Tiling             ,
        [<Optional;DefaultParameterValue(null)>]?PathBar            ,
        [<Optional;DefaultParameterValue(null)>]?Level              ,
        [<Optional;DefaultParameterValue(null)>]?Maxdepth           ,
        [<Optional;DefaultParameterValue(null)>]?Colors: seq<string>,
        [<Optional;DefaultParameterValue(null)>]?Colorbar:Colorbar
        ) =
        Trace.initTreemap(
            TraceStyle.Treemap(
                labels          = labels,
                parents         = parents,
                ?Ids            = Ids,
                ?Values         = Values,
                ?Text           = Text,
                ?Branchvalues   = Branchvalues,
                ?Tiling         = Tiling,
                ?PathBar        = PathBar,
                ?Level          = Level,
                ?Maxdepth       = Maxdepth
            )
        )
        |> TraceStyle.Marker(?Colors=Colors,?Colorbar=Colorbar)
        |> GenericChart.ofTraceObject

    /// Creates an OHLC (open-high-low-close) chart. OHLC charts are typically used to illustrate movements in the price of a financial instrument over time.
    ///
    /// ``open``    : Sets the open values.
    ///
    /// high        : Sets the high values.
    ///
    /// low         : Sets the low values.
    ///
    /// close       : Sets the close values.
    ///
    /// x           : Sets the x coordinates. If absent, linear coordinate will be generated.
    ///
    /// ?Increasing : Sets the Line style of the Increasing part of the chart
    ///
    /// ?Decreasing : Sets the Line style of the Decreasing part of the chart
    ///
    /// ?Line       : Sets the Line style of both the Decreasing and Increasing part of the chart
    ///
    /// ?Tickwidth  : Sets the width of the open/close tick marks relative to the "x" minimal interval.
    ///
    /// ?XCalendar  : Sets the calendar system to use with `x` date data.
    static member OHLC
        (
            ``open``        : #IConvertible seq,
            high            : #IConvertible seq,
            low             : #IConvertible seq,
            close           : #IConvertible seq,
            x               : #IConvertible seq,
            [<Optional;DefaultParameterValue(null)>]?Increasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?Decreasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?Tickwidth      : float,
            [<Optional;DefaultParameterValue(null)>]?Line           : Line,
            [<Optional;DefaultParameterValue(null)>]?XCalendar      : StyleParam.Calendar
        ) =
            Trace.initOHLC(
                TraceStyle.OHLC(
                    ``open``        = ``open``    ,
                    high            = high        ,
                    low             = low         ,
                    close           = close       ,
                    x               = x           ,
                    ?Increasing     = Increasing  ,
                    ?Decreasing     = Decreasing  ,
                    ?Tickwidth      = Tickwidth   ,
                    ?Line           = Line        ,
                    ?XCalendar      = XCalendar   
                )
            )
            |> GenericChart.ofTraceObject

    /// Creates an OHLC (open-high-low-close) chart. OHLC charts are typically used to illustrate movements in the price of a financial instrument over time.
    ///
    /// stockTimeSeries : tuple list of time * stock (OHLC) data
    ///
    /// ?Increasing     : Sets the Line style of the Increasing part of the chart
    ///
    /// ?Decreasing     : Sets the Line style of the Decreasing part of the chart
    ///
    /// ?Line           : Sets the Line style of both the Decreasing and Increasing part of the chart
    ///
    /// ?Tickwidth      : Sets the width of the open/close tick marks relative to the "x" minimal interval.
    ///
    /// ?XCalendar      : Sets the calendar system to use with `x` date data.
    static member OHLC
        (
            stockTimeSeries: seq<System.DateTime*StockData>, 
            [<Optional;DefaultParameterValue(null)>]?Increasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?Decreasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?Tickwidth      : float,
            [<Optional;DefaultParameterValue(null)>]?Line           : Line,
            [<Optional;DefaultParameterValue(null)>]?XCalendar      : StyleParam.Calendar
        ) =
            Trace.initOHLC(
                TraceStyle.OHLC(
                    ``open``        = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.Open)))    ,
                    high            = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.High)))        ,
                    low             = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.Low)))         ,
                    close           = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.Close)))       ,
                    x               = (stockTimeSeries |> Seq.map fst)            ,
                    ?Increasing     = Increasing  ,
                    ?Decreasing     = Decreasing  ,
                    ?Tickwidth      = Tickwidth   ,
                    ?Line           = Line        ,
                    ?XCalendar      = XCalendar   
                )
            )
            |> GenericChart.ofTraceObject

    /// Creates a candlestick chart. A candlestick cart is a style of financial chart used to describe price movements of a 
    /// security, derivative, or currency. Each "candlestick" typically shows one day, thus a one-month chart may show the 20 
    /// trading days as 20 candlesticks. Candlestick charts can also be built using intervals shorter or longer than one day.
    ///
    /// ``open``        : Sets the open values.
    ///
    /// high            : Sets the high values.
    ///
    /// low             : Sets the low values.
    ///
    /// close           : Sets the close values.
    ///
    /// x               : Sets the x coordinates. If absent, linear coordinate will be generated.
    ///
    /// ?Increasing     : Sets the Line style of the Increasing part of the chart
    ///
    /// ?Decreasing     : Sets the Line style of the Decreasing part of the chart
    ///
    /// ?Line           : Sets the Line style of both the Decreasing and Increasing part of the chart
    ///
    /// ?WhiskerWidth   :  Sets the width of the whiskers relative to the box' width. For example, with 1, the whiskers are as wide as the box(es).
    ///
    /// ?XCalendar      : Sets the calendar system to use with `x` date data.
    static member Candlestick
        (
            ``open``        : #IConvertible seq,
            high            : #IConvertible seq,
            low             : #IConvertible seq,
            close           : #IConvertible seq,
            x               : #IConvertible seq,
            [<Optional;DefaultParameterValue(null)>]?Increasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?Decreasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?WhiskerWidth   : float,
            [<Optional;DefaultParameterValue(null)>]?Line           : Line,
            [<Optional;DefaultParameterValue(null)>]?XCalendar      : StyleParam.Calendar
        ) =
            Trace.initCandlestick(
                TraceStyle.Candlestick(
                    ``open``        = ``open``    ,
                    high            = high        ,
                    low             = low         ,
                    close           = close       ,
                    x               = x           ,
                    ?Increasing     = Increasing  ,
                    ?Decreasing     = Decreasing  ,
                    ?WhiskerWidth   = WhiskerWidth,
                    ?Line           = Line        ,
                    ?XCalendar      = XCalendar   
                )
            )
            |> GenericChart.ofTraceObject

    /// Creates an OHLC (open-high-low-close) chart. OHLC charts are typically used to illustrate movements in the price of a financial instrument over time.
    ///
    /// stockTimeSeries : tuple list of time * stock (OHLC) data
    ///
    /// ?Increasing     : Sets the Line style of the Increasing part of the chart
    ///
    /// ?Decreasing     : Sets the Line style of the Decreasing part of the chart
    ///
    /// ?Line           : Sets the Line style of both the Decreasing and Increasing part of the chart
    ///
    /// ?Tickwidth      : Sets the width of the open/close tick marks relative to the "x" minimal interval.
    ///
    /// ?XCalendar      : Sets the calendar system to use with `x` date data.
    static member Candlestick
        (
            stockTimeSeries: seq<System.DateTime*StockData>, 
            [<Optional;DefaultParameterValue(null)>]?Increasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?Decreasing     : Line,
            [<Optional;DefaultParameterValue(null)>]?WhiskerWidth   : float,
            [<Optional;DefaultParameterValue(null)>]?Line           : Line,
            [<Optional;DefaultParameterValue(null)>]?XCalendar      : StyleParam.Calendar
        ) =
            Trace.initCandlestick(
                TraceStyle.Candlestick(
                    ``open``        = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.Open)))    ,
                    high            = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.High)))        ,
                    low             = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.Low)))         ,
                    close           = (stockTimeSeries |> Seq.map (snd >> (fun x -> x.Close)))       ,
                    x               = (stockTimeSeries |> Seq.map fst)            ,
                    ?Increasing     = Increasing  ,
                    ?Decreasing     = Decreasing  ,
                    ?WhiskerWidth   = WhiskerWidth,
                    ?Line           = Line        ,
                    ?XCalendar      = XCalendar   
                )
            )
            |> GenericChart.ofTraceObject


    /// Creates a waterfall chart. Waterfall charts are special bar charts that help visualizing the cumulative effect of sequentially introduced positive or negative values
    ///
    /// Parameters:
    ///
    /// x               : Sets the x coordinates.
    ///
    /// y               : Sets the y coordinates.
    ///
    /// Base            : Sets where the bar base is drawn (in position axis units).
    ///
    /// Width           : Sets the bar width (in position axis units).
    ///
    /// Measure         : An array containing types of values. By default the values are considered as 'relative'. However; it is possible to use 'total' to compute the sums. Also 'absolute' could be applied to reset the computed total or to declare an initial value where needed.
    ///
    /// Orientation     : Sets the orientation of the bars. With "v" ("h"), the value of the each bar spans along the vertical (horizontal).
    ///
    /// Connector       : Sets the styling of the connector lines
    ///
    /// AlignmentGroup  : Set several traces linked to the same position axis or matching axes to the same alignmentgroup. This controls whether bars compute their positional range dependently or independently.
    ///
    /// OffsetGroup     : Set several traces linked to the same position axis or matching axes to the same offsetgroup where bars of the same position coordinate will line up.
    ///
    /// Offset          : Shifts the position where the bar is drawn (in position axis units). In "group" barmode, traces that set "offset" will be excluded and drawn in "overlay" mode instead.
    static member Waterfall 
        (
            x               : #IConvertible seq,
            y               : #IConvertible seq,
            [<Optional;DefaultParameterValue(null)>]?Base           : IConvertible  ,
            [<Optional;DefaultParameterValue(null)>]?Width          : float         ,
            [<Optional;DefaultParameterValue(null)>]?Measure        : StyleParam.WaterfallMeasure seq,
            [<Optional;DefaultParameterValue(null)>]?Orientation    : StyleParam.Orientation,
            [<Optional;DefaultParameterValue(null)>]?Connector      : WaterfallConnector    ,
            [<Optional;DefaultParameterValue(null)>]?AlignmentGroup : string,
            [<Optional;DefaultParameterValue(null)>]?OffsetGroup    : string,
            [<Optional;DefaultParameterValue(null)>]?Offset
        ) =
            Trace.initWaterfall(
                TraceStyle.Waterfall(x,y,
                    ?Base           = Base          ,
                    ?Width          = Width         ,
                    ?Measure        = Measure       ,
                    ?Orientation    = Orientation   ,
                    ?Connector      = Connector     ,
                    ?AlignmentGroup = AlignmentGroup,
                    ?OffsetGroup    = OffsetGroup   ,
                    ?Offset         = Offset        
                )
            )
            |> GenericChart.ofTraceObject


    /// Creates a waterfall chart. Waterfall charts are special bar charts that help visualizing the cumulative effect of sequentially introduced positive or negative values
    ///
    /// Parameters:
    ///
    /// xyMeasures      : triple sequence containing x coordinates, y coordinates, and the type of measure used for each bar.
    ///
    /// Base            : Sets where the bar base is drawn (in position axis units).
    ///
    /// Width           : Sets the bar width (in position axis units).
    ///
    /// Orientation     : Sets the orientation of the bars. With "v" ("h"), the value of the each bar spans along the vertical (horizontal).
    ///
    /// Connector       : Sets the styling of the connector lines
    ///
    /// AlignmentGroup  : Set several traces linked to the same position axis or matching axes to the same alignmentgroup. This controls whether bars compute their positional range dependently or independently.
    ///
    /// OffsetGroup     : Set several traces linked to the same position axis or matching axes to the same offsetgroup where bars of the same position coordinate will line up.
    ///
    /// Offset          : Shifts the position where the bar is drawn (in position axis units). In "group" barmode, traces that set "offset" will be excluded and drawn in "overlay" mode instead.
    static member Waterfall 
        (
            xyMeasure: (#IConvertible*#IConvertible*StyleParam.WaterfallMeasure) seq,
            [<Optional;DefaultParameterValue(null)>]?Base           : IConvertible  ,
            [<Optional;DefaultParameterValue(null)>]?Width          : float         ,
            [<Optional;DefaultParameterValue(null)>]?Orientation    : StyleParam.Orientation,
            [<Optional;DefaultParameterValue(null)>]?Connector      : WaterfallConnector    ,
            [<Optional;DefaultParameterValue(null)>]?AlignmentGroup : string,
            [<Optional;DefaultParameterValue(null)>]?OffsetGroup    : string,
            [<Optional;DefaultParameterValue(null)>]?Offset
        ) =
            let x,y,measure = Seq.unzip3 xyMeasure
            Trace.initWaterfall(
                TraceStyle.Waterfall(x,y,
                    ?Base           = Base          ,
                    ?Width          = Width         ,
                    ?Measure        = Some measure  ,
                    ?Orientation    = Orientation   ,
                    ?Connector      = Connector     ,
                    ?AlignmentGroup = AlignmentGroup,
                    ?OffsetGroup    = OffsetGroup   ,
                    ?Offset         = Offset        
                )
            )
            |> GenericChart.ofTraceObject

    /// Creates a Funnel chart.
    /// Funnel charts visualize stages in a process using length-encoded bars. This trace can be used to show data in either a part-to-whole representation wherein each item appears in a single stage, or in a "drop-off" representation wherein each item appears in each stage it traversed. See also the "funnelarea" trace type for a different approach to visualizing funnel data.
    ///
    /// Parameters:
    /// 
    /// x              : Sets the x coordinates.
    ///
    /// y              : Sets the y coordinates.
    ///
    /// Name           : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend     : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// Opacity        : Sets the Opacity of the trace
    ///
    /// Labels         : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition   : Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont       : Sets the text font of this trace
    ///
    /// Color          : Sets Marker Color
    ///
    /// Line           : Line type
    ///
    /// x0             : Alternate to `x`. Builds a linear space of x coordinates. Use with `dx` where `x0` is the starting coordinate and `dx` the step.
    ///
    /// dX             : Sets the x coordinate step. See `x0` for more info.
    ///
    /// y0             : Alternate to `y`. Builds a linear space of y coordinates. Use with `dy` where `y0` is the starting coordinate and `dy` the step.
    ///
    /// dY             : Sets the y coordinate step. See `y0` for more info.
    ///
    /// Width          : Sets the bar width (in position axis units).
    /// 
    /// Offset         : Shifts the position where the bar is drawn (in position axis units). In "group" barmode, traces that set "offset" will be excluded and drawn in "overlay" mode instead.
    /// 
    /// Orientation    : Sets the orientation of the funnels. With "v" ("h"), the value of the each bar spans along the vertical (horizontal). By default funnels are tend to be oriented horizontally; unless only "y" array is presented or orientation is set to "v". Also regarding graphs including only 'horizontal' funnels, "autorange" on the "y-axis" are set to "reversed".
    /// 
    /// Alignmentgroup : Set several traces linked to the same position axis or matching axes to the same alignmentgroup. This controls whether bars compute their positional range dependently or independently.
    /// 
    /// Offsetgroup    : Set several traces linked to the same position axis or matching axes to the same offsetgroup where bars of the same position coordinate will line up.
    /// 
    /// Cliponaxis     : Determines whether the text nodes are clipped about the subplot axes. To show the text nodes above axis lines and tick labels, make sure to set `xaxis.layer` and `yaxis.layer` to "below traces".
    /// 
    /// Connector      : Connector type
    ///
    /// Insidetextfont : Sets the font used for `text` lying inside the bar.
    ///
    /// Outsidetextfont: Sets the font used for `text` lying outside the bar.
    static member Funnel (x, y,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Line                          ,
            [<Optional;DefaultParameterValue(null)>] ?x0                            ,
            [<Optional;DefaultParameterValue(null)>] ?dX                            ,
            [<Optional;DefaultParameterValue(null)>] ?y0                            ,
            [<Optional;DefaultParameterValue(null)>] ?dY                            ,
            [<Optional;DefaultParameterValue(null)>] ?Width                         ,
            [<Optional;DefaultParameterValue(null)>] ?Offset                        ,
            [<Optional;DefaultParameterValue(null)>] ?Orientation                   ,
            [<Optional;DefaultParameterValue(null)>] ?Alignmentgroup                ,
            [<Optional;DefaultParameterValue(null)>] ?Offsetgroup                   ,
            [<Optional;DefaultParameterValue(null)>] ?Cliponaxis                    ,
            [<Optional;DefaultParameterValue(null)>] ?Connector                     ,
            [<Optional;DefaultParameterValue(null)>] ?Insidetextfont                ,
            [<Optional;DefaultParameterValue(null)>] ?Outsidetextfont
        ) = 

        Trace.initFunnel(
            TraceStyle.Funnel(
                x               = x               ,
                y               = y               ,
                ?x0              = x0             ,
                ?dX              = dX             ,
                ?y0              = y0             ,
                ?dY              = dY             ,
                ?Width           = Width          ,
                ?Offset          = Offset         ,
                ?Orientation     = Orientation    ,
                ?Alignmentgroup  = Alignmentgroup ,
                ?Offsetgroup     = Offsetgroup    ,
                ?Cliponaxis      = Cliponaxis     ,
                ?Connector       = Connector      ,
                ?Insidetextfont  = Insidetextfont ,
                ?Outsidetextfont = Outsidetextfont
            )
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Line=Line)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a FunnelArea chart.
    /// FunnelArea charts visualize stages in a process using area-encoded trapezoids. This trace can be used to show data in a part-to-whole representation similar to a "pie" trace, wherein each item appears in a single stage. See also the "funnel" trace type for a different approach to visualizing funnel data.
    ///
    /// Parameters:
    /// 
    /// Values        : Sets the values of the sectors. If omitted, we count occurrences of each label.
    ///
    /// Labels        : Sets the sector labels. If `labels` entries are duplicated, we sum associated `values` or simply count occurrences if `values` is not provided. For other array attributes (including color) we use the first non-empty entry among all occurrences of the label.
    ///
    /// dLabel        : Sets the label step. See `label0` for more info.
    ///
    /// Label0        : Alternate to `labels`. Builds a numeric set of labels. Use with `dlabel` where `label0` is the starting label and `dlabel` the step.
    ///
    /// Name          : Sets the trace name. The trace name appear as the legend item and on hover.
    ///
    /// Showlegend    : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// Opacity       : Sets the opacity of the trace.
    ///
    /// Color         : Sets Marker Color
    ///
    /// Line          : Line type
    ///
    /// Text          : Sets text elements associated with each sector. If trace `textinfo` contains a "text" flag, these elements will be seen on the chart. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition  : Specifies the location of the `textinfo`.
    ///
    /// X             : Sets the horizontal domain of this funnelarea trace (in plot fraction).
    ///
    /// Y             : Sets the vertical domain of this funnelarea trace (in plot fraction).
    ///
    /// Row           : If there is a layout grid, use the domain for this row in the grid for this funnelarea trace .
    ///
    /// Column        : If there is a layout grid, use the domain for this column in the grid for this funnelarea trace .
    ///
    /// Aspectratio   : Sets the ratio between height and width
    ///
    /// Baseratio     : Sets the ratio between bottom length and maximum top length.
    ///
    /// Insidetextfont: Sets the font used for `textinfo` lying inside the sector.
    ///
    /// Scalegroup    : If there are multiple funnelareas that should be sized according to their totals, link them by providing a non-empty group id here shared by every trace in the same group.
    static member FunnelArea 
        (
            [<Optional;DefaultParameterValue(null)>] ?Values        ,
            [<Optional;DefaultParameterValue(null)>] ?Labels        ,
            [<Optional;DefaultParameterValue(null)>] ?dLabel        ,
            [<Optional;DefaultParameterValue(null)>] ?Label0        ,
            [<Optional;DefaultParameterValue(null)>] ?Name          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend    ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity       ,
            [<Optional;DefaultParameterValue(null)>] ?Color         ,
            [<Optional;DefaultParameterValue(null)>] ?Line          ,
            [<Optional;DefaultParameterValue(null)>] ?Text          ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition  ,
            [<Optional;DefaultParameterValue(null)>] ?X             ,
            [<Optional;DefaultParameterValue(null)>] ?Y             ,
            [<Optional;DefaultParameterValue(null)>] ?Row           ,
            [<Optional;DefaultParameterValue(null)>] ?Column        ,
            [<Optional;DefaultParameterValue(null)>] ?Aspectratio   ,
            [<Optional;DefaultParameterValue(null)>] ?Baseratio     ,
            [<Optional;DefaultParameterValue(null)>] ?Insidetextfont,
            [<Optional;DefaultParameterValue(null)>] ?Scalegroup
        ) = 

        Trace.initFunnelArea(
            TraceStyle.FunnelArea(
                ?Values         = Values        ,
                ?Labels         = Labels        ,
                ?dLabel         = dLabel        ,
                ?Label0         = Label0        ,
                ?Aspectratio    = Aspectratio   ,
                ?Baseratio      = Baseratio     ,
                ?Insidetextfont = Insidetextfont,
                ?Scalegroup     = Scalegroup
            )
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Line=Line)
        |> TraceStyle.Domain(?X=X,?Y=Y,?Row=Row,?Column=Column)
        |> TraceStyle.TextLabel(?Text=Text,?Textposition=TextPosition)
        |> GenericChart.ofTraceObject

    /// Creates a ScatterGeo chart, where data is visualized on a geographic map.
    /// ScatterGeo charts are the basis of GeoPoint, GeoLine, and GeoBubble Charts, and can be customized as such. We also provide abstractions for those: Chart.GeoPoint, Chart.GeoLine, Chart.GeoBubble
    ///
    /// Parameters:
    /// 
    /// longitudes  : Sets the longitude coordinates (in degrees East).
    ///
    /// latitudes   : Sets the latitude coordinates (in degrees North).
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// Dash        : Sets the Line Dash style
    ///
    /// Width       : Sets the Line width
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member ScatterGeo(longitudes, latitudes, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?Dash                          ,
            [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = mode          ,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject


    /// Creates a ScatterGeo chart, where data is visualized on a geographic map.
    /// ScatterGeo charts are the basis of GeoPoint, GeoLine, and GeoBubble Charts, and can be customized as such. We also provide abstractions for those: Chart.GeoPoint, Chart.GeoLine, Chart.GeoBubble
    ///
    /// Parameters:
    ///
    /// lonlat      : Sets the (longitude,latitude) coordinates (in degrees North, degrees South).
    ///
    /// mode        : Determines the drawing mode for this scatter trace.
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// Dash        : Sets the Line Dash style
    ///
    /// Width       : Sets the Line width
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member ScatterGeo(lonlat, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?Dash                          ,
            [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 
        let longitudes, latitudes = Seq.unzip lonlat

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = mode          ,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a ScatterGeo chart, where data is visualized on a geographic map.
    /// ScatterGeo charts are the basis of GeoPoint, GeoLine, and GeoBubble Charts, and can be customized as such. We also provide abstractions for those: Chart.GeoPoint, Chart.GeoLine, Chart.GeoBubble
    ///
    /// Parameters:
    ///
    /// locations   : Sets the coordinates via location IDs or names. Coordinates correspond to the centroid of each location given. See `locationmode` for more info.
    ///
    /// mode        : Determines the drawing mode for this scatter trace.
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// Dash        : Sets the Line Dash style
    ///
    /// Width       : Sets the Line width
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member ScatterGeo(locations, mode,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?Dash                          ,
            [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = mode          ,
                ?Locations      = locations     ,
                ?GeoJson        = GeoJson       ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps   ,
                ?Fill           = Fill          ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Dash=Dash,?Width=Width)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a PointGeo chart, where data is visualized as points on a geographic map.
    ///
    /// Parameters:
    /// 
    /// longitudes  : Sets the longitude coordinates (in degrees East).
    ///
    /// latitudes   : Sets the latitude coordinates (in degrees North).
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member PointGeo(longitudes, latitudes,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 

        let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = changeMode StyleParam.Mode.Markers ,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a PointGeo chart, where data is visualized as points on a geographic map.
    ///
    /// Parameters:
    /// 
    /// lonlat      : Sets the (longitude,latitude) coordinates (in degrees North, degrees South).
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member PointGeo(lonlat,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor    
        ) = 

        let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)
        let longitudes, latitudes = Seq.unzip lonlat

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = changeMode StyleParam.Mode.Markers ,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a PointGeo chart, where data is visualized as points on a geographic map.
    ///
    /// Parameters:
    ///
    /// locations   : Sets the coordinates via location IDs or names. Coordinates correspond to the centroid of each location given. See `locationmode` for more info.
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (x,y) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member PointGeo(locations,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor    
        ) = 
        
        let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode        = changeMode StyleParam.Mode.Markers ,
                ?Locations  = locations     ,
                ?GeoJson    = GeoJson       ,
                ?Connectgaps= Connectgaps   ,
                ?Fill       = Fill          ,
                ?Fillcolor  = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a LineGeo chart, where data is visualized as coordinates connected via lines on a geographic map.
    ///
    /// Parameters:
    /// 
    /// longitudes  : Sets the longitude coordinates (in degrees East).
    ///
    /// latitudes   : Sets the latitude coordinates (in degrees North).
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// ShowMarkers : Determines wether or not markers will be rendered for each datum.
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (lon,lat) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// Dash        : Sets the Line Dash style
    ///
    /// Width       : Sets the Line width
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member LineGeo(longitudes, latitudes,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers                   ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor    
        ) = 

        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = changeMode StyleParam.Mode.Lines,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a LineGeo chart, where data is visualized as coordinates connected via lines on a geographic map.
    ///
    /// Parameters:
    /// 
    /// lonlat      : Sets the (longitude,latitude) coordinates (in degrees North, degrees South).
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// ShowMarkers : Determines wether or not markers will be rendered for each datum.
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (lon,lat) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.
    static member LineGeo(lonlat,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers                   ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor    
        ) = 

        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)
        let longitudes, latitudes = Seq.unzip lonlat

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = changeMode StyleParam.Mode.Lines,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// Creates a LineGeo chart, where data is visualized as coordinates connected via lines on a geographic map.
    ///
    /// Parameters:
    ///
    /// locations   : Sets the coordinates via location IDs or names. Coordinates correspond to the centroid of each location given. See `locationmode` for more info.
    ///
    /// Name        : Sets the trace name. The trace name appear as the legend item and on hover
    ///
    /// ShowMarkers : Determines wether or not markers will be rendered for each datum.
    ///
    /// Showlegend  : Determines whether or not an item corresponding to this trace is shown in the legend.
    ///
    /// MarkerSymbol: Sets the type of symbol that datums are displayed as
    ///
    /// Color       : Sets Line/Marker Color
    ///
    /// Opacity     : Sets the Opacity of the trace
    ///
    /// Labels      : Sets text elements associated with each (x,y) pair. If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (x,y) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.
    ///
    /// TextPosition: Sets the positions of the `text` elements with respects to the (lon,lat) coordinates.
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// TextFont    : Sets the text font of this trace
    ///
    /// GeoJson     : Sets optional GeoJSON data associated with this trace. If not given, the features on the base map are used when `locations` is set. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".
    ///
    /// Connectgaps : Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.
    ///
    /// Fill        : Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.
    ///
    /// Fillcolor   : Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.

    static member LineGeo(locations,
            [<Optional;DefaultParameterValue(null)>] ?Name                          ,
            [<Optional;DefaultParameterValue(null)>] ?Showlegend                    ,
            [<Optional;DefaultParameterValue(null)>] ?MarkerSymbol                  ,
            [<Optional;DefaultParameterValue(null)>] ?ShowMarkers                   ,
            [<Optional;DefaultParameterValue(null)>] ?Color                         ,
            [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
            [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
            [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
            [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
            [<Optional;DefaultParameterValue(null)>] ?GeoJson                       ,
            [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey: string          ,
            [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
            [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
            [<Optional;DefaultParameterValue(null)>] ?Fillcolor    
        ) = 

        let changeMode = 
            let isShowMarker =
                match ShowMarkers with
                | Some isShow -> isShow
                | Option.None        -> false
            StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
            >> StyleParam.ModeUtils.showMarker (isShowMarker)

        Trace.initScatterGeo(
            TraceStyle.ScatterGeo(
                mode            = changeMode StyleParam.Mode.Lines,
                Locations       = locations    ,
                ?GeoJson        = GeoJson      ,
                ?FeatureIdKey   = FeatureIdKey ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=Showlegend,?Opacity=Opacity)
        |> TraceStyle.Marker(?Color=Color,?Symbol=MarkerSymbol)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject

    /// <summary>
    /// Creates a ScatterMapbox chart, where data is visualized by (longitude,latitude) pairs on a geographic map using mapbox.
    ///
    /// Customize the mapbox layers, style, etc. by using Chart.withMapbox.
    ///
    /// You might need a Mapbox token, which you can also configure with Chart.withMapbox.
    ///
    /// ScatterGeo charts are the basis of PointMapbox and LineMapbox Charts, and can be customized as such. We also provide abstractions for those: Chart.PointMapbox and Chart.LineMapbox
    /// </summary>
    /// <param name="longitudes">Sets the longitude coordinates (in degrees East).</param>
    /// <param name="latitudes">Sets the latitude coordinates (in degrees North).</param>
    /// <param name="mode">Determines the drawing mode for this scatter trace. If the provided `mode` includes "text" then the `text` elements appear at the coordinates. Otherwise, the `text` elements appear on hover.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover.</param>
    /// <param name="ShowLegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="Color">Sets the marker color. It accepts either a specific color or an array of numbers that are mapped to the colorscale relative to the max and min values of the array or relative to `marker.cmin` and `marker.cmax` if set.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Labels">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the icon text font (color=mapbox.layer.paint.text-color, size=mapbox.layer.layout.text-size). Has an effect only when `type` is set to "symbol".</param>
    /// <param name="Width">Sets the line width (in px).</param>
    /// <param name="Below">Determines if this scattermapbox trace's layers are to be inserted before the layer with the specified ID. By default, scattermapbox layers are inserted above all the base layers. To place the scattermapbox layers above every other layer, set `below` to "''".</param>
    /// <param name="Connectgaps">Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.</param>
    /// <param name="Fill">Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.</param>
    /// <param name="Fillcolor">Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.</param>
    static member ScatterMapbox(longitudes, latitudes, mode,
        [<Optional;DefaultParameterValue(null)>] ?Name                          ,
        [<Optional;DefaultParameterValue(null)>] ?ShowLegend                    ,
        [<Optional;DefaultParameterValue(null)>] ?Color                         ,
        [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
        [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
        [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
        [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
        [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
        [<Optional;DefaultParameterValue(null)>] ?Below : string                ,
        [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
        [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
        [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 

        Trace.initScatterMapbox(
            TraceStyle.ScatterMapbox(
                mode            = mode          ,
                Longitudes      = longitudes    ,
                Latitudes       = latitudes     ,
                ?Below          = Below         ,
                ?Connectgaps    = Connectgaps  ,
                ?Fill           = Fill         ,
                ?Fillcolor      = Fillcolor    
            )               
        )
        |> TraceStyle.TraceInfo(?Name=Name,?Showlegend=ShowLegend,?Opacity=Opacity)
        |> TraceStyle.Line(?Color=Color,?Width=Width)
        |> TraceStyle.Marker(?Color=Color)
        |> TraceStyle.TextLabel(?Text=Labels,?Textposition=TextPosition,?Textfont=TextFont)
        |> GenericChart.ofTraceObject 

    /// <summary>
    /// Creates a ScatterMapbox chart, where data is visualized by (longitude,latitude) pairs on a geographic map using mapbox.
    ///
    /// Customize the mapbox layers, style, etc. by using Chart.withMapbox.
    ///
    /// You might need a Mapbox token, which you can also configure with Chart.withMapbox.
    ///
    /// ScatterGeo charts are the basis of PointMapbox and LineMapbox Charts, and can be customized as such. We also provide abstractions for those: Chart.PointMapbox and Chart.LineMapbox
    /// </summary>
    /// <param name="lonlat">Sets the (longitude,latitude) coordinates (in degrees North, degrees South).</param>
    /// <param name="mode">Determines the drawing mode for this scatter trace. If the provided `mode` includes "text" then the `text` elements appear at the coordinates. Otherwise, the `text` elements appear on hover.</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover.</param>
    /// <param name="ShowLegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="Color">Sets the marker color. It accepts either a specific color or an array of numbers that are mapped to the colorscale relative to the max and min values of the array or relative to `marker.cmin` and `marker.cmax` if set.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Labels">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the icon text font (color=mapbox.layer.paint.text-color, size=mapbox.layer.layout.text-size). Has an effect only when `type` is set to "symbol".</param>
    /// <param name="Width">Sets the line width (in px).</param>
    /// <param name="Below">Determines if this scattermapbox trace's layers are to be inserted before the layer with the specified ID. By default, scattermapbox layers are inserted above all the base layers. To place the scattermapbox layers above every other layer, set `below` to "''".</param>
    /// <param name="Connectgaps">Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.</param>
    /// <param name="Fill">Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.</param>
    /// <param name="Fillcolor">Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.</param>
    static member ScatterMapbox(lonlat, mode,
        [<Optional;DefaultParameterValue(null)>] ?Name                          ,
        [<Optional;DefaultParameterValue(null)>] ?ShowLegend                    ,
        [<Optional;DefaultParameterValue(null)>] ?Color                         ,
        [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
        [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
        [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
        [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
        [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
        [<Optional;DefaultParameterValue(null)>] ?Below : string                ,
        [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
        [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
        [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 

            let longitudes, latitudes = Seq.unzip lonlat

            Chart.ScatterMapbox(
                longitudes, 
                latitudes, 
                mode,
                ?Name        =  Name       ,
                ?ShowLegend  =  ShowLegend ,
                ?Color       =  Color      ,
                ?Opacity     =  Opacity    ,
                ?Labels      =  Labels     ,
                ?TextPosition=  TextPosition,
                ?TextFont    =  TextFont   ,
                ?Width       =  Width      ,
                ?Below       =  Below      ,
                ?Connectgaps =  Connectgaps,
                ?Fill        =  Fill       ,
                ?Fillcolor   =  Fillcolor  
            )                  
                           
    /// <summary>
    /// Creates a PointMapbox chart, where data is visualized by (longitude,latitude) pairs as Points on a geographic map using mapbox.
    ///
    /// Customize the mapbox layers, style, etc. by using Chart.withMapbox.
    ///
    /// You might need a Mapbox token, which you can also configure with Chart.withMapbox.
    /// </summary>
    /// <param name="longitudes">Sets the longitude coordinates (in degrees East).</param>
    /// <param name="latitudes">Sets the latitude coordinates (in degrees North).</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover.</param>
    /// <param name="ShowLegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="Color">Sets the marker color. It accepts either a specific color or an array of numbers that are mapped to the colorscale relative to the max and min values of the array or relative to `marker.cmin` and `marker.cmax` if set.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Labels">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the icon text font (color=mapbox.layer.paint.text-color, size=mapbox.layer.layout.text-size). Has an effect only when `type` is set to "symbol".</param>
    /// <param name="Width">Sets the line width (in px).</param>
    /// <param name="Below">Determines if this scattermapbox trace's layers are to be inserted before the layer with the specified ID. By default, scattermapbox layers are inserted above all the base layers. To place the scattermapbox layers above every other layer, set `below` to "''".</param>
    /// <param name="Connectgaps">Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.</param>
    /// <param name="Fill">Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.</param>
    /// <param name="Fillcolor">Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.</param>
    static member PointMapbox(longitudes,latitudes,
        [<Optional;DefaultParameterValue(null)>] ?Name                          ,
        [<Optional;DefaultParameterValue(null)>] ?ShowLegend                    ,
        [<Optional;DefaultParameterValue(null)>] ?Color                         ,
        [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
        [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
        [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
        [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
        [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
        [<Optional;DefaultParameterValue(null)>] ?Below : string                ,
        [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
        [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
        [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 
            
            let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)

            Chart.ScatterMapbox(
                longitudes, 
                latitudes, 
                mode = changeMode StyleParam.Mode.Markers ,
                ?Name        =  Name       ,
                ?ShowLegend  =  ShowLegend ,
                ?Color       =  Color      ,
                ?Opacity     =  Opacity    ,
                ?Labels      =  Labels     ,
                ?TextPosition=  TextPosition,
                ?TextFont    =  TextFont   ,
                ?Width       =  Width      ,
                ?Below       =  Below      ,
                ?Connectgaps =  Connectgaps,
                ?Fill        =  Fill       ,
                ?Fillcolor   =  Fillcolor  
            )                  
                                                      
    /// <summary>
    /// Creates a PointMapbox chart, where data is visualized by (longitude,latitude) pairs as Points on a geographic map using mapbox.
    ///
    /// Customize the mapbox layers, style, etc. by using Chart.withMapbox.
    ///
    /// You might need a Mapbox token, which you can also configure with Chart.withMapbox.
    /// </summary>
    /// <param name="lonlat">Sets the (longitude,latitude) coordinates (in degrees North, degrees South).</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover.</param>
    /// <param name="ShowLegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="Color">Sets the marker color. It accepts either a specific color or an array of numbers that are mapped to the colorscale relative to the max and min values of the array or relative to `marker.cmin` and `marker.cmax` if set.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Labels">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the icon text font (color=mapbox.layer.paint.text-color, size=mapbox.layer.layout.text-size). Has an effect only when `type` is set to "symbol".</param>
    /// <param name="Width">Sets the line width (in px).</param>
    /// <param name="Below">Determines if this scattermapbox trace's layers are to be inserted before the layer with the specified ID. By default, scattermapbox layers are inserted above all the base layers. To place the scattermapbox layers above every other layer, set `below` to "''".</param>
    /// <param name="Connectgaps">Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.</param>
    /// <param name="Fill">Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.</param>
    /// <param name="Fillcolor">Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.</param>
    static member PointMapbox(lonlat,
        [<Optional;DefaultParameterValue(null)>] ?Name                          ,
        [<Optional;DefaultParameterValue(null)>] ?ShowLegend                    ,
        [<Optional;DefaultParameterValue(null)>] ?Color                         ,
        [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
        [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
        [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
        [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
        [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
        [<Optional;DefaultParameterValue(null)>] ?Below : string                ,
        [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
        [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
        [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 
            
            let changeMode = StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)
            let longitudes, latitudes = Seq.unzip lonlat

            Chart.ScatterMapbox(
                longitudes, 
                latitudes, 
                mode = changeMode StyleParam.Mode.Markers ,
                ?Name        =  Name       ,
                ?ShowLegend  =  ShowLegend ,
                ?Color       =  Color      ,
                ?Opacity     =  Opacity    ,
                ?Labels      =  Labels     ,
                ?TextPosition=  TextPosition,
                ?TextFont    =  TextFont   ,
                ?Width       =  Width      ,
                ?Below       =  Below      ,
                ?Connectgaps =  Connectgaps,
                ?Fill        =  Fill       ,
                ?Fillcolor   =  Fillcolor  
            )                                             
    /// <summary>
    /// Creates a LineMapbox chart, where data is visualized by (longitude,latitude) pairs connected by a line on a geographic map using mapbox.
    ///
    /// Customize the mapbox layers, style, etc. by using Chart.withMapbox.
    ///
    /// You might need a Mapbox token, which you can also configure with Chart.withMapbox.
    /// </summary>
    /// <param name="longitudes">Sets the longitude coordinates (in degrees East).</param>
    /// <param name="latitudes">Sets the latitude coordinates (in degrees North).</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover.</param>
    /// <param name="ShowLegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="ShowMarkers">Determines whether or not To show markers for the individual datums.</param>
    /// <param name="Color">Sets the marker color. It accepts either a specific color or an array of numbers that are mapped to the colorscale relative to the max and min values of the array or relative to `marker.cmin` and `marker.cmax` if set.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Labels">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the icon text font (color=mapbox.layer.paint.text-color, size=mapbox.layer.layout.text-size). Has an effect only when `type` is set to "symbol".</param>
    /// <param name="Width">Sets the line width (in px).</param>
    /// <param name="Below">Determines if this scattermapbox trace's layers are to be inserted before the layer with the specified ID. By default, scattermapbox layers are inserted above all the base layers. To place the scattermapbox layers above every other layer, set `below` to "''".</param>
    /// <param name="Connectgaps">Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.</param>
    /// <param name="Fill">Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.</param>
    /// <param name="Fillcolor">Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.</param>
    static member LineMapbox(longitudes,latitudes,
        [<Optional;DefaultParameterValue(null)>] ?Name                          ,
        [<Optional;DefaultParameterValue(null)>] ?ShowLegend                    ,
        [<Optional;DefaultParameterValue(null)>] ?ShowMarkers                   ,
        [<Optional;DefaultParameterValue(null)>] ?Color                         ,
        [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
        [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
        [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
        [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
        [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
        [<Optional;DefaultParameterValue(null)>] ?Below : string                ,
        [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
        [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
        [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 
            
            
            let changeMode = 
                let isShowMarker =
                    match ShowMarkers with
                    | Some isShow -> isShow
                    | Option.None        -> false
                StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
                >> StyleParam.ModeUtils.showMarker (isShowMarker)

            Chart.ScatterMapbox(
                longitudes, 
                latitudes, 
                mode = changeMode StyleParam.Mode.Lines ,
                ?Name        =  Name       ,
                ?ShowLegend  =  ShowLegend ,
                ?Color       =  Color      ,
                ?Opacity     =  Opacity    ,
                ?Labels      =  Labels     ,
                ?TextPosition=  TextPosition,
                ?TextFont    =  TextFont   ,
                ?Width       =  Width      ,
                ?Below       =  Below      ,
                ?Connectgaps =  Connectgaps,
                ?Fill        =  Fill       ,
                ?Fillcolor   =  Fillcolor  
            )                  
                                                      
    /// <summary>
    /// Creates a LineMapbox chart, where data is visualized by (longitude,latitude) pairs connected by a line on a geographic map using mapbox.
    ///
    /// Customize the mapbox layers, style, etc. by using Chart.withMapbox.
    ///
    /// You might need a Mapbox token, which you can also configure with Chart.withMapbox.
    /// </summary>
    /// <param name="lonlat">Sets the (longitude,latitude) coordinates (in degrees North, degrees South).</param>
    /// <param name="Name">Sets the trace name. The trace name appear as the legend item and on hover.</param>
    /// <param name="ShowLegend">Determines whether or not an item corresponding to this trace is shown in the legend.</param>
    /// <param name="ShowMarkers">Determines whether or not To show markers for the individual datums.</param>
    /// <param name="Color">Sets the marker color. It accepts either a specific color or an array of numbers that are mapped to the colorscale relative to the max and min values of the array or relative to `marker.cmin` and `marker.cmax` if set.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Labels">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="TextPosition">Sets the positions of the `text` elements with respects to the (x,y) coordinates.</param>
    /// <param name="TextFont">Sets the icon text font (color=mapbox.layer.paint.text-color, size=mapbox.layer.layout.text-size). Has an effect only when `type` is set to "symbol".</param>
    /// <param name="Width">Sets the line width (in px).</param>
    /// <param name="Below">Determines if this scattermapbox trace's layers are to be inserted before the layer with the specified ID. By default, scattermapbox layers are inserted above all the base layers. To place the scattermapbox layers above every other layer, set `below` to "''".</param>
    /// <param name="Connectgaps">Determines whether or not gaps (i.e. {nan} or missing values) in the provided data arrays are connected.</param>
    /// <param name="Fill">Sets the area to fill with a solid color. Use with `fillcolor` if not "none". "toself" connects the endpoints of the trace (or each segment of the trace if it has gaps) into a closed shape.</param>
    /// <param name="Fillcolor">Sets the fill color. Defaults to a half-transparent variant of the line color, marker color, or marker line color, whichever is available.</param>
    static member LineMapbox(lonlat,
        [<Optional;DefaultParameterValue(null)>] ?Name                          ,
        [<Optional;DefaultParameterValue(null)>] ?ShowLegend                    ,
        [<Optional;DefaultParameterValue(null)>] ?ShowMarkers                   ,
        [<Optional;DefaultParameterValue(null)>] ?Color                         ,
        [<Optional;DefaultParameterValue(null)>] ?Opacity                       ,
        [<Optional;DefaultParameterValue(null)>] ?Labels                        ,
        [<Optional;DefaultParameterValue(null)>] ?TextPosition                  ,
        [<Optional;DefaultParameterValue(null)>] ?TextFont                      ,
        [<Optional;DefaultParameterValue(null)>] ?Width : float                 ,
        [<Optional;DefaultParameterValue(null)>] ?Below : string                ,
        [<Optional;DefaultParameterValue(null)>] ?Connectgaps : bool            ,
        [<Optional;DefaultParameterValue(null)>] ?Fill        : StyleParam.Fill ,
        [<Optional;DefaultParameterValue(null)>] ?Fillcolor   
        ) = 
            
            let changeMode = 
                let isShowMarker =
                    match ShowMarkers with
                    | Some isShow -> isShow
                    | Option.None        -> false
                StyleParam.ModeUtils.showText (TextPosition.IsSome || TextFont.IsSome)                       
                >> StyleParam.ModeUtils.showMarker (isShowMarker)
            let longitudes, latitudes = Seq.unzip lonlat

            Chart.ScatterMapbox(
                longitudes, 
                latitudes, 
                mode = changeMode StyleParam.Mode.Lines ,
                ?Name        =  Name       ,
                ?ShowLegend  =  ShowLegend ,
                ?Color       =  Color      ,
                ?Opacity     =  Opacity    ,
                ?Labels      =  Labels     ,
                ?TextPosition=  TextPosition,
                ?TextFont    =  TextFont   ,
                ?Width       =  Width      ,
                ?Below       =  Below      ,
                ?Connectgaps =  Connectgaps,
                ?Fill        =  Fill       ,
                ?Fillcolor   =  Fillcolor  
            )                  

    /// <summary>
    /// Creates a ChoroplethMapbox Chart. 
    ///
    /// Choropleth Maps display divided geographical areas or regions that are coloured, shaded or patterned in relation to 
    /// a data variable. This provides a way to visualise values over a geographical area, which can show variation or 
    /// patterns across the displayed location.
    ///
    /// GeoJSON features to be filled are set in `geojson` The data that describes the choropleth value-to-color mapping is set in `locations` and `z`.
    /// </summary>
    /// <param name="locations">Sets which features found in "geojson" to plot using their feature `id` field.</param>
    /// <param name="z">Sets the color values.</param>
    /// <param name="geoJson">Sets the GeoJSON data associated with this trace. It can be set as a valid GeoJSON object or as a URL string. Note that we only accept GeoJSONs of type "FeatureCollection" or "Feature" with geometries of type "Polygon" or "MultiPolygon".</param>
    /// <param name="FeatureIdKey">Sets the key in GeoJSON features which is used as id to match the items included in the `locations` array. Support nested property, for example "properties.name".</param>
    /// <param name="Text">Sets the text elements associated with each location.</param>
    /// <param name="Below">Determines if the choropleth polygons will be inserted before the layer with the specified ID. By default, choroplethmapbox traces are placed above the water layers. If set to '', the layer will be inserted above every existing layer.</param>
    /// <param name="Colorscale">Sets the colorscale.</param>
    /// <param name="Colorbar">Sets the Colorbar object asociated with this trace</param>
    /// <param name="ZAuto">Determines whether or not the color domain is computed with respect to the input data (here in `z`) or the bounds set in `zmin` and `zmax` Defaults to `false` when `zmin` and `zmax` are set by the user.</param>
    /// <param name="ZMin">Sets the lower bound of the color domain. Value should have the same units as in `z` and if set, `zmax` must be set as well.</param>
    /// <param name="ZMid">Sets the mid-point of the color domain by scaling `zmin` and/or `zmax` to be equidistant to this point. Value should have the same units as in `z`. Has no effect when `zauto` is `false`.</param>
    /// <param name="ZMax">Sets the upper bound of the color domain. Value should have the same units as in `z` and if set, `zmin` must be set as well.</param>
    static member ChoroplethMapbox(locations,z,geoJson,
        [<Optional;DefaultParameterValue(null)>] ?FeatureIdKey,
        [<Optional;DefaultParameterValue(null)>] ?Text,
        [<Optional;DefaultParameterValue(null)>] ?Below,
        [<Optional;DefaultParameterValue(null)>] ?Colorscale,
        [<Optional;DefaultParameterValue(null)>] ?Colorbar,
        [<Optional;DefaultParameterValue(null)>] ?ZAuto,
        [<Optional;DefaultParameterValue(null)>] ?ZMin,
        [<Optional;DefaultParameterValue(null)>] ?ZMid,
        [<Optional;DefaultParameterValue(null)>] ?ZMax
        ) =
            
            Trace.initChoroplethMapbox (
                TraceStyle.ChoroplethMapbox (
                    Z               = z,
                    Locations       = locations,
                    GeoJson         = geoJson,
                    ?FeatureIdKey   = FeatureIdKey,
                    ?Text           = Text,
                    ?Below          = Below,
                    ?Colorscale     = Colorscale,
                    ?Colorbar       = Colorbar,
                    ?ZAuto          = ZAuto,
                    ?ZMin           = ZMin,
                    ?ZMid           = ZMid,
                    ?ZMax           = ZMax
                )
            )
            |> GenericChart.ofTraceObject
            
    /// <summary>
    /// Creates a DensityMapbox Chart that draws a bivariate kernel density estimation with a Gaussian kernel from `lon` and `lat` coordinates and optional `z` values using a colorscale.
    /// </summary>
    /// <param name="lon">Sets the longitude coordinates (in degrees East).</param>
    /// <param name="lat">Sets the latitude coordinates (in degrees North).</param>
    /// <param name="Z">Sets the points' weight. For example, a value of 10 would be equivalent to having 10 points of weight 1 in the same spot</param>
    /// <param name="Radius">Sets the radius of influence of one `lon` / `lat` point in pixels. Increasing the value makes the densitymapbox trace smoother, but less detailed.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Text">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="Below">Determines if the densitymapbox trace will be inserted before the layer with the specified ID. By default, densitymapbox traces are placed below the first layer of type symbol If set to '', the layer will be inserted above every existing layer.</param>
    /// <param name="Colorscale">Sets the colorscale.</param>
    /// <param name="Colorbar">Sets the Colorbar object asociated with this trace</param>
    /// <param name="Showscale">Determines whether or not a colorbar is displayed for this trace.</param>
    /// <param name="ZAuto">Determines whether or not the color domain is computed with respect to the input data (here in `z`) or the bounds set in `zmin` and `zmax` Defaults to `false` when `zmin` and `zmax` are set by the user.</param>
    /// <param name="ZMin">Sets the lower bound of the color domain. Value should have the same units as in `z` and if set, `zmax` must be set as well.</param>
    /// <param name="ZMid">Sets the mid-point of the color domain by scaling `zmin` and/or `zmax` to be equidistant to this point. Value should have the same units as in `z`. Has no effect when `zauto` is `false`.</param>
    /// <param name="ZMax">Sets the upper bound of the color domain. Value should have the same units as in `z` and if set, `zmin` must be set as well.</param>
    static member DensityMapbox (lon,lat,
        [<Optional;DefaultParameterValue(null)>] ?Z,
        [<Optional;DefaultParameterValue(null)>] ?Radius,
        [<Optional;DefaultParameterValue(null)>] ?Opacity,
        [<Optional;DefaultParameterValue(null)>] ?Text,
        [<Optional;DefaultParameterValue(null)>] ?Below,
        [<Optional;DefaultParameterValue(null)>] ?Colorscale,
        [<Optional;DefaultParameterValue(null)>] ?Colorbar,
        [<Optional;DefaultParameterValue(null)>] ?Showscale ,
        [<Optional;DefaultParameterValue(null)>] ?ZAuto,
        [<Optional;DefaultParameterValue(null)>] ?ZMin,
        [<Optional;DefaultParameterValue(null)>] ?ZMid,
        [<Optional;DefaultParameterValue(null)>] ?ZMax
        ) =
            Trace.initDensityMapbox(
                TraceStyle.DensityMapbox(
                    Longitudes  = lon,
                    Latitudes   = lat,
                    ?Z          = Z,
                    ?Radius     = Radius,
                    ?Opacity    = Opacity,
                    ?Text       = Text,
                    ?Below      = Below,
                    ?Colorscale = Colorscale,
                    ?Colorbar   = Colorbar,
                    ?Showscale  = Showscale,
                    ?ZAuto      = ZAuto,
                    ?ZMin       = ZMin,
                    ?ZMid       = ZMid,
                    ?ZMax       = ZMax
                )
            )
            |> GenericChart.ofTraceObject
    
    /// <summary>
    /// Creates a DensityMapbox Chart that draws a bivariate kernel density estimation with a Gaussian kernel from `lon` and `lat` coordinates and optional `z` values using a colorscale.
    /// </summary>
    /// <param name="lonlat">Sets the (longitude,latitude) coordinates (in degrees North, degrees South).</param>
    /// <param name="Z">Sets the points' weight. For example, a value of 10 would be equivalent to having 10 points of weight 1 in the same spot</param>
    /// <param name="Radius">Sets the radius of influence of one `lon` / `lat` point in pixels. Increasing the value makes the densitymapbox trace smoother, but less detailed.</param>
    /// <param name="Opacity">Sets the opacity of the trace.</param>
    /// <param name="Text">Sets text elements associated with each (lon,lat) pair If a single string, the same string appears over all the data points. If an array of string, the items are mapped in order to the this trace's (lon,lat) coordinates. If trace `hoverinfo` contains a "text" flag and "hovertext" is not set, these elements will be seen in the hover labels.</param>
    /// <param name="Below">Determines if the densitymapbox trace will be inserted before the layer with the specified ID. By default, densitymapbox traces are placed below the first layer of type symbol If set to '', the layer will be inserted above every existing layer.</param>
    /// <param name="Colorscale">Sets the colorscale.</param>
    /// <param name="Colorbar">Sets the Colorbar object asociated with this trace</param>
    /// <param name="Showscale">Determines whether or not a colorbar is displayed for this trace.</param>
    /// <param name="ZAuto">Determines whether or not the color domain is computed with respect to the input data (here in `z`) or the bounds set in `zmin` and `zmax` Defaults to `false` when `zmin` and `zmax` are set by the user.</param>
    /// <param name="ZMin">Sets the lower bound of the color domain. Value should have the same units as in `z` and if set, `zmax` must be set as well.</param>
    /// <param name="ZMid">Sets the mid-point of the color domain by scaling `zmin` and/or `zmax` to be equidistant to this point. Value should have the same units as in `z`. Has no effect when `zauto` is `false`.</param>
    /// <param name="ZMax">Sets the upper bound of the color domain. Value should have the same units as in `z` and if set, `zmin` must be set as well.</param>
    static member DensityMapbox (lonlat,
        [<Optional;DefaultParameterValue(null)>] ?Z,
        [<Optional;DefaultParameterValue(null)>] ?Radius,
        [<Optional;DefaultParameterValue(null)>] ?Opacity,
        [<Optional;DefaultParameterValue(null)>] ?Text,
        [<Optional;DefaultParameterValue(null)>] ?Below,
        [<Optional;DefaultParameterValue(null)>] ?Colorscale,
        [<Optional;DefaultParameterValue(null)>] ?Colorbar,
        [<Optional;DefaultParameterValue(null)>] ?Showscale ,
        [<Optional;DefaultParameterValue(null)>] ?ZAuto,
        [<Optional;DefaultParameterValue(null)>] ?ZMin,
        [<Optional;DefaultParameterValue(null)>] ?ZMid,
        [<Optional;DefaultParameterValue(null)>] ?ZMax
        ) = 

            let longitudes, latitudes = Seq.unzip lonlat
            
            Chart.DensityMapbox(
                longitudes,
                latitudes,
                ?Z          = Z,
                ?Radius     = Radius,
                ?Opacity    = Opacity,
                ?Text       = Text,
                ?Below      = Below,
                ?Colorscale = Colorscale,
                ?Colorbar   = Colorbar, 
                ?Showscale  = Showscale,
                ?ZAuto      = ZAuto,
                ?ZMin       = ZMin,
                ?ZMid       = ZMid,
                ?ZMax       = ZMax      
            )