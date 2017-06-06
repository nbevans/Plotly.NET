(*** hide ***)
#r "../../bin/Newtonsoft.Json.dll"
#r "../../bin/FSharp.Plotly.dll"

(** 
# FSharp.Plotly: Pie and Doughnut Charts

*Summary:* This example shows how to create pie and doughnut charts in F#.

A pie or a doughnut chart can be created using the `Chart.Pie` and `Chart.Doughnut` functions.
When creating pie or doughnut charts, it is usually desirable to provide both labels and 
values.
*)

open FSharp.Plotly 


let normal (rnd:System.Random) mu tau =
    let mutable v1 = 2.0 * rnd.NextDouble() - 1.0
    let mutable v2 = 2.0 * rnd.NextDouble() - 1.0
    let mutable r = v1 * v1 + v2 * v2
    while (r >= 1.0 || r = 0.0) do
        v1 <- 2.0 * rnd.NextDouble() - 1.0
        v2 <- 2.0 * rnd.NextDouble() - 1.0
        r <- v1 * v1 + v2 * v2
    let fac = sqrt(-2.0*(log r)/r)
    (tau * v1 * fac + mu)

let rnd = System.Random()
let sampleNormal () = 
    normal (rnd) 0. 2.

let n = 2000
let a = -1.
let b = 1.2
let step i = a +  ((b - a) / float (n - 1)) * float i

let x = Array.init n (fun i -> ((step i)**3.) + (0.3 * sampleNormal () ))
let y = Array.init n (fun i -> ((step i)**6.) + (0.3 * sampleNormal () ))


let colorScale = StyleParam.Colorscale.Custom [(0.0,"white");(1.0,"red")]

(*** define-output:Histogram2dContour1 ***)
[
    Chart.Histogram2dContour (x,y,Colorscale=colorScale,Line=Line.init(Width=0))
    Chart.Point(x,y,Opacity=0.3)
]
|> Chart.Combine
(*** include-it:Histogram2dContour1 ***)
|> Chart.Show


(*** define-output:Histogram1 ***)
[
    //var layout = {barmode: "overlay"};
    Chart.Histogram x
    Chart.Histogram y
]
|> Chart.Combine
(*** include-it:Histogram1 ***)
|> Chart.Show


(*** define-output:Histogram2d1 ***)
Chart.Histogram2d (x,y,Colorscale=colorScale)
(*** include-it:Histogram2d1 ***)
|> Chart.Show


