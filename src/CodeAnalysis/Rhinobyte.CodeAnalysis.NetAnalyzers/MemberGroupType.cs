namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Member group type values used to specify expected member ordering
/// </summary>
public enum MemberGroupType
{
	Unknown = 0,
	Constants = 1,
	Constructors = 2,
	InstanceMethods = 3,
	InstanceProperties = 4,
	MutableInstanceFields = 5,
	NestedEnumType = 6,
	NestedOtherType = 7,
	NestedRecordType = 8,
	ReadonlyInstanceFields = 9,
	StaticConstructors = 10,
	StaticMethods = 11,
	StaticMutableFields = 12,
	StaticProperties = 13,
	StaticReadonlyFields = 14
}
