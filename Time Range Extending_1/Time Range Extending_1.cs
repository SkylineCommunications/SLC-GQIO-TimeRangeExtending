using System;
using Skyline.DataMiner.Analytics.GenericInterface;

[GQIMetaData(Name = "Time Range Feed Extending")]
public class TimeRangeExtending : IGQIRowOperator, IGQIInputArguments, IGQIColumnOperator
{
    private readonly GQIColumnDropdownArgument _startColumnArg = new GQIColumnDropdownArgument("Start") { IsRequired = true, Types = new GQIColumnType[] { GQIColumnType.DateTime } };
    private readonly GQIColumnDropdownArgument _endColumnArg = new GQIColumnDropdownArgument("End") { IsRequired = true, Types = new GQIColumnType[] { GQIColumnType.DateTime } };
    private readonly GQIIntArgument _HoursBeforeColumnArg = new GQIIntArgument("Amount of hours before") { IsRequired = true };
    private readonly GQIIntArgument _HoursAfterColumnArg = new GQIIntArgument("Amount of hours after") { IsRequired = true };

    private readonly GQIDateTimeColumn _TimeFeedStart = new GQIDateTimeColumn("TimeRange Feed Start");
    private readonly GQIDateTimeColumn _TimeFeedEnd = new GQIDateTimeColumn("TimeRange Feed End");


    private GQIEditableDateTimeColumn _startColumn;
    private GQIEditableDateTimeColumn _endColumn;
    private TimeSpan _TimeRangeBefore;
    private TimeSpan _TimeRangeAfter;


    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { _startColumnArg, _endColumnArg, _HoursBeforeColumnArg, _HoursAfterColumnArg };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _startColumn = args.GetArgumentValue(_startColumnArg) as GQIEditableDateTimeColumn;
        _endColumn = args.GetArgumentValue(_endColumnArg) as GQIEditableDateTimeColumn;

        _TimeRangeBefore = new TimeSpan(args.GetArgumentValue(_HoursBeforeColumnArg), 0, 0);
        _TimeRangeAfter = new TimeSpan(args.GetArgumentValue(_HoursAfterColumnArg), 0, 0);

        return new OnArgumentsProcessedOutputArgs();
    }

    public void HandleColumns(GQIEditableHeader header)
    {
        header.AddColumns(_TimeFeedStart);
        header.AddColumns(_TimeFeedEnd);
    }

    public void HandleRow(GQIEditableRow row)
    {
        try
        {
            DateTime start = (row.GetValue(_startColumn) - _TimeRangeBefore);
            DateTime end = (row.GetValue(_endColumn) + _TimeRangeAfter);

            // Max out to now
            if (end > DateTime.UtcNow)
                end = DateTime.UtcNow;

            // in case you want to have the updated values in the same columns
            row.SetValue<DateTime>(_TimeFeedStart, start);
            row.SetValue<DateTime>(_TimeFeedEnd, end);

            // in case we would support multiple timerange feeds at some point
            //var timeRangeMetadata = new TimeRangeMetadata {StartTime = start -_TimeRangeBefore, EndTime = end + _TimeRangeAfter};
            /* if (row.Metadata != null && row.Metadata.Metadata != null)
                AddItemToArray(row.Metadata.Metadata, timeRangeMetadata);*/


            if (row.Metadata != null && row.Metadata.Metadata != null)
            {
                foreach (var metaData in row.Metadata.Metadata)
                {
                    if (metaData is TimeRangeMetadata)
                    {
                        (metaData as TimeRangeMetadata).StartTime = start;
                        (metaData as TimeRangeMetadata).EndTime = end;
                    }
                }
            }
        }
        catch (Exception ex) { }
    }


    public static RowMetadataBase[] AddItemToArray(RowMetadataBase[] array, RowMetadataBase newItem)
    {
        // Create a new array with one extra slot
        RowMetadataBase[] newArray = new RowMetadataBase[array.Length + 1];

        // Copy existing items to the new array
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }

        // Add the new item to the end of the new array
        newArray[array.Length] = newItem;

        return newArray;
    }

}