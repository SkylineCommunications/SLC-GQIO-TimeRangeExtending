using System;
using Skyline.DataMiner.Analytics.GenericInterface;

[GQIMetaData(Name = "Time Range Feed Extending")]
public class TimeRangeExtending : IGQIRowOperator, IGQIInputArguments, IGQIColumnOperator
{
    private readonly GQIColumnDropdownArgument _startColumnArg = new GQIColumnDropdownArgument("Start") { IsRequired = true, Types = new GQIColumnType[] { GQIColumnType.DateTime } };
    private readonly GQIColumnDropdownArgument _endColumnArg = new GQIColumnDropdownArgument("End") { IsRequired = true, Types = new GQIColumnType[] { GQIColumnType.DateTime } };
    private readonly GQIIntArgument _hoursBeforeColumnArg = new GQIIntArgument("Amount of hours before") { IsRequired = true };
    private readonly GQIIntArgument _hoursAfterColumnArg = new GQIIntArgument("Amount of hours after") { IsRequired = true };

    private readonly GQIDateTimeColumn _timeFeedStart = new GQIDateTimeColumn("TimeRange Feed Start");
    private readonly GQIDateTimeColumn _timeFeedEnd = new GQIDateTimeColumn("TimeRange Feed End");

    private GQIEditableDateTimeColumn _startColumn;
    private GQIEditableDateTimeColumn _endColumn;
    private TimeSpan _timeRangeBefore;
    private TimeSpan _timeRangeAfter;

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { _startColumnArg, _endColumnArg, _hoursBeforeColumnArg, _hoursAfterColumnArg };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        _startColumn = args.GetArgumentValue(_startColumnArg) as GQIEditableDateTimeColumn;
        _endColumn = args.GetArgumentValue(_endColumnArg) as GQIEditableDateTimeColumn;

        _timeRangeBefore = new TimeSpan(args.GetArgumentValue(_hoursBeforeColumnArg), 0, 0);
        _timeRangeAfter = new TimeSpan(args.GetArgumentValue(_hoursAfterColumnArg), 0, 0);

        return new OnArgumentsProcessedOutputArgs();
    }

    public void HandleColumns(GQIEditableHeader header)
    {
        header.AddColumns(_timeFeedStart);
        header.AddColumns(_timeFeedEnd);
    }

    public void HandleRow(GQIEditableRow row)
    {
        try
        {
            DateTime start = row.GetValue(_startColumn) - _timeRangeBefore;
            DateTime end = row.GetValue(_endColumn) + _timeRangeAfter;

            // Max out to now
            if (end > DateTime.UtcNow)
                end = DateTime.UtcNow;

            // in case you want to have the updated values in the same columns
            row.SetValue<DateTime>(_timeFeedStart, start);
            row.SetValue<DateTime>(_timeFeedEnd, end);

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
        catch (Exception)
		{
		}
    }
}