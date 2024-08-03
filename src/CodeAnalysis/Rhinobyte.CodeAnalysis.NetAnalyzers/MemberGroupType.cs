namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Member group type values used to specify expected member ordering
/// </summary>
public enum MemberGroupType
{
	/// <summary>
	/// Unknown value declared as the default to ensure the enum is not unintionally defaulted to an actual group.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Member group type for defining the group order placement of <c>const</c> members.
	/// </summary>
	Constants = 1,

	/// <summary>
	/// Member group type for defining the group order placement of constructors.
	/// </summary>
	Constructors = 2,

	/// <summary>
	/// Member group type for defining the group order placement of instance methods.
	/// </summary>
	InstanceMethods = 3,

	/// <summary>
	/// Member group type for defining the group order placement of instance properties.
	/// </summary>
	InstanceProperties = 4,

	/// <summary>
	/// Member group type for defining the group order placement of mutable instance fields.
	/// </summary>
	MutableInstanceFields = 5,

	/// <summary>
	/// Member group type for defining the group order placement of nested enum types.
	/// </summary>
	NestedEnumType = 6,

	/// <summary>
	/// Member group type for defining the group order placement of nested other types.
	/// </summary>
	NestedOtherType = 7,

	/// <summary>
	/// Member group type for defining the group order placement of nested record types.
	/// </summary>
	NestedRecordType = 8,

	/// <summary>
	/// Member group type for defining the group order placement of readonly instance fields.
	/// </summary>
	ReadonlyInstanceFields = 9,

	/// <summary>
	/// Member group type for defining the group order placement of static constructors.
	/// </summary>
	StaticConstructors = 10,

	/// <summary>
	/// Member group type for defining the group order placement of static methods.
	/// </summary>
	StaticMethods = 11,

	/// <summary>
	/// Member group type for defining the group order placement of static mutable fields.
	/// </summary>
	StaticMutableFields = 12,

	/// <summary>
	/// Member group type for defining the group order placement of static properties.
	/// </summary>
	StaticProperties = 13,

	/// <summary>
	/// Member group type for defining the group order placement of static readonly fields.
	/// </summary>
	StaticReadonlyFields = 14
}
