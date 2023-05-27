
## Namespaces:

The tests follow a peculiar namespacing rule. 
Everything is in the same primary namespace, and subnamespaces follow.
This is done to reduce the number of unnecessary folds in the test runner.

`<module>_Tests_(EditMode|PlayMode).<subnamespace1>.<subnamespace2>`

Examples of correct primary namespaces:
- `KSSCore_Tests_EditMode`
- `KSS_Tests_EditMode`
- `KSSUI_Tests_EditMode`

## Test Naming:

Names follow the structure
`<tested method/feature>___<initial state>___<expected behavior>`

Triple underscores (`___`) are for legibility in the Unity test runner again.

If the initial state consists of many distinct elements, a single underscore (`_`) can be used to separate them.

Examples of correct method names:
- `Empty___ShouldBeEmpty`
- `Add___SingleSubstance_NegativeDt___RemovesFromExisting`






