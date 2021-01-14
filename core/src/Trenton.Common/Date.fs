namespace Trenton.Common

open System
open System.Collections
open System.Runtime.Serialization

// Based off of https://github.com/supersonicclay/csharp-date
[<RequireQualifiedAccess>]
module Date =
    type DateParts = { Year: int; Month: int; Day: int }

    [<CustomEquality; CustomComparison>]
    type T =
        private
        | Date of DateParts

        member private this._dt =
            match this with
            | Date d -> DateTime(d.Year, d.Month, d.Day)

        interface IStructuralComparable with
            member this.CompareTo(other, comparer) =
                comparer.Compare(this._dt, other)

        interface ISerializable with
            member this.GetObjectData(info, _) =
                info.AddValue("ticks", this._dt.Ticks)

        member this.ToShortString = this._dt.ToShortDateString

        member this.ToLongString = this._dt.ToLongDateString

        override this.ToString() = this.ToShortString()

        member this.ToString(provider: IFormatProvider) =
            this._dt.ToString provider

        member this.ToString(fmt: string) =
            match fmt with
            | "O"
            | "o"
            | "s" -> this.ToString "yyyy-MM-dd"
            | _ -> this.ToString fmt

        override this.Equals d = this._dt.Equals d

        override this.GetHashCode() = this._dt.GetHashCode()

        member this.DayOfWeek = this._dt.DayOfWeek

        member this.DayOfYear = this._dt.DayOfYear

        member this.Ticks = this._dt.Ticks

        member this.ToDateTime() = this._dt

        static member FromDateTime(dt: DateTime) =
            Date
                { Year = dt.Day
                  Month = dt.Month
                  Day = dt.Day }

        static member (-)(d1: T, d2: T) = d1._dt - d2._dt

        static member (-)(d: T, t: TimeSpan) = d._dt - t |> T.FromDateTime

        static member (+)(d: T, t: TimeSpan) = d._dt + t |> T.FromDateTime

    let create y m d =
        try
            DateTime(y, m, d) |> T.FromDateTime |> Some
        with _ -> None

    let today () = T.FromDateTime <| DateTime.Now

    let addDays (d: T) days =
        d.ToDateTime().AddDays(days) |> T.FromDateTime

    let addMonths (d: T) months =
        d.ToDateTime().AddMonths(months) |> T.FromDateTime

    let addYears (d: T) years =
        d.ToDateTime().AddYears(years) |> T.FromDateTime

    let daysInMonth y m = DateTime.DaysInMonth(y, m)

    let parse s = s |> DateTime.Parse |> T.FromDateTime

    let tryParse (s: string) =
        match DateTime.TryParse s with
        | (false, _) -> None
        | (true, d) -> T.FromDateTime d |> Some
