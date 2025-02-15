namespace Application.Common.DapperQueries;

public static class Constants
{
    public static class TFieldType
    {
        public const string TString = "string";
        public const string TBool = "bool";
        //public const string TSelect = "select";
        //public const string TMultiSelect = "multiselect";
        public const string TDate = "date";
        public const string TDateTime = "datetime";
        public const string TDateRange = "daterange";
        public const string TNumber = "number";
        public const string TTime = "time";
        public const string TGuid = "guid";
    }

    public static class TFilterType
    {
        public const string TString = "string";
        public const string TBool = "bool";
        public const string TSelect = "select";
        public const string TMultiSelect = "multiselect";
        public const string TDate = "date";
        public const string TDateTime = "datetime";
        public const string TDateRange = "daterange";
    }

    public static class MatchMode
    {
        public const string STARTS_WITH = "startsWith";
        public const string ENDS_WITH = "endsWith";
        public const string CONTAINS = "contains";
        public const string NOT_CONTAINS = "notContains";
        public const string EQUALS = "equals";
        public const string NOT_EQUALS = "notEquals";
        public const string IN = "in";
        public const string NOTIN = "notin";
        public const string DATE_IS = "dateIs";
        public const string DATE_IS_NOT = "dateIsNot";
        public const string DATE_BEFORE = "dateBefore";
        public const string DATE_AFTER = "dateAfter";
        public const string DATE_IS_OR_AFTER = "dateIsOrAfter";
        public const string DATE_IS_OR_BEFORE = "dateIsOrBefore";
        public const string LESS_THAN = "lt";
        public const string LESS_THAN_OR_EQUAL_TO = "lte";
        public const string GREATER_THAN = "gt";
        public const string GREATER_THAN_OR_EQUAL_TO = "gte";
        public const string BETWEEN = "between";
        public const string IS = "is";
        public const string IS_NOT = "isNot";
        public const string BEFORE = "before";
        public const string AFTER = "after";

    }
}

public static class SqlConstants
{
    public static class S
    {
        public const string LIKE = nameof(LIKE);
        public const string AND = nameof(AND);
        public const string OR = nameof(OR);
        public const string EQUAL = nameof(EQUAL);

        public const string SELECT = nameof(SELECT);
        public const string FROM = nameof(FROM);
        public const string WHERE = nameof(WHERE);
        public const string ORDERBY = "ORDER BY";
        public const string GROUPBY = "GROUP BY";
        public const string COUNT = nameof(COUNT);
        public const string HAVING = nameof(HAVING);
        public const string INSERT = nameof(INSERT);
        public const string DELETE = nameof(DELETE);
        public const string UPDATE = nameof(UPDATE);
        public const string TOP = nameof(TOP);
        public const string LEFT = nameof(LEFT);
        public const string RIGHT = nameof(RIGHT);
        public const string NULL = nameof(NULL);
        public const string JOIN = nameof(JOIN);
        public const string L_JOIN = "LEFT JOIN";
        public const string R_JOIN = "RIGHT JOIN";
        public const string ASC = nameof(ASC);
        public const string DESC = nameof(DESC);
        public const string OVER = nameof(OVER);
        public const string IN = nameof(IN);
        public const string O_JOIN = "OUT JOIN";
        public const string UPPER = nameof(UPPER);
        public const string LOWER = nameof(LOWER);
        public const string NOT = nameof(NOT);
        public const string CONV = "CONVERT";
        public const string IS = "IS";
        public const string IS_NOT = "IS NOT";
    }
}
