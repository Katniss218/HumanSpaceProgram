#ONLY SAVES	// attribute, tells the engine to only run the script in the `SAVES` context
#ONLY SAVES, GAMEDATA	// list attribute

// path starts at the root directory, and to filter anything, you must first navigate to the file that interests you.
// the files are stripped of their file extensions, if two files with the same name match, both will be targeted.
// FOR selects FOR the current object(s)
// WHERE filters FOR the current object(s)

// "..." means select by name
// [...] means select by index
// @/.../ means select by regex on the name or index (index will be converted to a string to match the regex)


// `$` is the 'value of' operator - means we're getting the value . No $ means we're passing in a literal.

(FOR "Vessels".[*]."gameobjects")	// [*] can be used in a dict as well, it will select every value. 
									// Here "Vessels" is a serializedobject containing serializedobjects corresponding to folders.
{
    // selects the 'root' serializeddata to be the one that directly contains the match.
    (WHERE $this."$type" == "value")
    {
        "$type" = "othervalue";	// this will create a key if it's not already there.
		
        "children"."some_other_value" = 5; // targets a nested value.
		
        "same.level" = "this_is_not_a_child_node";
    }
	
    // selects where the path matches, but without changing the root.
    (WHERE $"components".[*]."$type" == "value")
    {
        "components".[*]."$type" = "othervalue";
    }
    // equivalent to the above.
    // selectors and filters are applied FOR left to right. I.e. select entries matching "{this}.components.[*]", then filter by "{this}.$type".
    (FOR "components".[*] WHERE $"$type" == "value")
    {
		"$type" = "othervalue";
    }
    // equivalent to the above.
	(FOR "components".[*]."$type" WHERE $this == "value")
	{
		this = "othervalue";
	}
	
    // selects entries that match the path (i.e. ones that exist).
    (FOR "components".[1..5].[1..].[..5])
    {
		"temp" = this;
		"temp" += 5;
		
		this = $"temp";	// $ means we're dealing with a selectable object that has a value instead of a literal.
		
		//"temp" = null; not needed since 'this' being reassigned to an int cleared it.
		// delete "temp";
    }
	
	// @ means use regex for name/index resolution. (convert index to a string and check if it matches) 
	// SLOW
	(WHERE $@/comp*/.@/1-9/)
	{
	
	}
	
	(FOR "components".[0])
	{
		// first component.
		value = (FOR "children" WHERE "$type" == "somevalue").[0];	// copies the first element of "children" that has type "somevalue"
	}
	// equivalent to the above, but slower.
	(FOR (FOR "components").[0])
	{
	
	}

    // [*] means every object in a collection (array/object)
    // this means that the expressions are inherently tree-like and branching, producing more and more results in a flattenned list.
	
	// we want to have "" around names, they make it much easier to parse.
}

(FOR "vessels".[*]."gameobjects")
{
	// (nested) object constructor syntax
	this = 
	{
		"$type" = "somevalue",
		"$id" = "othervalue"
	};
	
	this = "value";	// this is also object constructor syntax, for a string.
	
	this = [ ]; // empty array literal.
	this = { }; // empty object literal.
	"$type" = "somevalue";
	"$id" = "othervalue";
}



// needs to support relative paths in the "FOR" and "where" clauses (i.e. no matter the location, if the name/index/value matches)

#FUNCTION
(ADD $"p1", $"p2")
{
	// 'this' is the value returned FROM the function.
	this = $"p1";
}

(FOR "vessels".[*]."gameobjects")
{
	"value" = (ADD $"$type", $"$id");
}

// ################################################################
// version 2

// support either [A-Za-z0-9_]* (minus keywords) or quoted keys, with dot preceeding a key access and [] for key, index, or range based access.
// if you want to set the value of a key that is also a keyword, use a quoted or [] access - `"this" = 5;` or `["this"] = 5;` or `this.this = 5;`
// if `this` is omitted from the beginning, it is implied.

// path expression examples:
a = 5;
a[0] = 5;
a[0].b = 5;
"hello world"[0] = 5;
"hello world"[0]."hi there" = 5;
"hello world"[0]["hi there"] = 5;
this = 5;
this.a = 5;
this.a = $a[0].b;

#FUNCTION
(ADD p1, p2) // parameter names are now specified as literals, without a $ prefix
{
	// 'this' is the value returned FROM the function.
	this = $p1 + $p2;
}

(FOR vessels[*].gameobjects)
{
	value = (ADD $"$type", $"$id");
	// equivalent to `this.value = ...;`
}



// passing in a function would be very useful.
// expressions that return/operate on multiple values would also be VERY useful.
// - they can be treated as an implicit array.

// `vessels[*].gameobjects` - expands to all array elements contained inside the vessels (none if vessels is not an array) that have a 'gameobjects' object key. sets that key as the pivot.

(FOR $vessels[*].gameobjects.any) // do whatever to find the thing
{
	//																		\/ index access here should be valid.
	// transformation header is an expression
	(FOR $this.components[*] WHERE $"$type" == "HSP.Vessels.Components.FPart").[0]
	{
		temp = $"$type";
		// variables could be done using a variable path segment maybe.
		
		this = (FOR $global.parts[*] WHERE $this."folder_name" == temp)[0]; // a way to access `temp` here would be nice too. a function would help, since it can be encoded as a parameter.
		
		// variables would also be nice. each scope is limited to the transformation body. parent scopes are accessible in child scopes.
		// having it all baked into the AST feels like it would be very clunky now.
		// - a separate interpreter that traverses the AST would be better.
		
		// a way to set something using a mapping would also be somewhat nice. as in, if the mapping is memberwise, it only replaces the members with names that are present in he mapping
		// - could be a builtin function
	}
}

// new change: FOR header needs an element / implicit arrayy of elements to operate on, use $ to evaluate the path and return an array. Requires `expressions that return/operate on multiple values`.

