namespace GALibrary
open System.Collections.Generic
open System

type Parameter<'T when 'T : enum<int> and 'T : equality and 'T : (static member op_Explict : 'T -> int)>(name:'T, max:single, min:single) =
    let mutable normalizedValue = 0.5f
    
    member this.Name = name
    member this.Max = max
    member this.Min = min
    
    member this.NormalizedValue
        with get() = normalizedValue
        and set(v) = normalizedValue <- v
    member this.GetValue() =  this.Min + (this.Max - this.Min) * this.NormalizedValue

    interface System.Collections.Generic.IEqualityComparer<Parameter<'T>> with
        member c.Equals (x, y) = x.Name = y.Name && x.NormalizedValue = y.NormalizedValue
        member c.GetHashCode x = 0
    //override this.Equals(o)

[<AbstractClassAttribute>]
type ItemBase<'T when 'T : enum<int> and 'T : equality and 'T : (static member op_Explict : 'T -> int)>(index : int) =
    member this.Index : int = index
    member this.Params = Array.zeroCreate<Parameter<'T>> (System.Enum.GetValues typeof<'T>).Length
    abstract CurrentScore : single with get
    member this.Item
        with get(i : 'T) = this.Params.[int i].GetValue()


type TestType(name) = 
    let mutable n = name
    member this.Name
        with get()  = n
        and set(v : 'T when 'T : enum<int>) = n <- v

module Module1 =
    let func (x : 'T when 'T : enum<_>) = x :> Enum
    let f2  (x : 'T when 'T : enum<int>) = x :> Enum
    let DecomposeFlag (value : 'T when 'T : enum<_>) : ISet<'T> =
        if Convert.ToUInt64(value) = 0uL
        then
            new HashSet<'T> ([value]) :> ISet<'T>
        else
            let has flag = (flag :> Enum).HasFlag
            let flags = Enum.GetValues typeof<'T>
                        |> Seq.cast<'T>
                        |> Seq.filter (fun flag -> Convert.ToUInt64 flag <> 0uL)
                        |> Seq.filter (has value)
            let result = flags
                        |> Seq.filter (fun flag -> Seq.forall (fun e -> e = flag || not (has flag e)) flags)
            new HashSet<'T> (result) :> ISet<'T>