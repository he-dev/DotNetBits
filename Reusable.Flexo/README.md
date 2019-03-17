# Flexo

## Expressions

- `Switch` - evaluates multiple cases and invokes the `Body` of the first match.
  - `Value` - an expression returning a `Constant<object>`
  - `Cases` - a collection of `SwitchCase` where
    - `When` - if `Constant` then `ObjectEqual` is used otherwise it must be a `Predicate`.
    - `Body` - any expression
  - `Default` - any expression that should be invoked when there was no match.
  - Context - `Value`

- `Contains`
  - `Comparer` - if `null` then `ObjectEqual` is used otherwise it must be a `Predicate`.
  - Context - `Value`, `Item`

- `Matches`
  - `IgnoreCase` - indicates whether `Regex` should ignore case. Default is `true`.
  - `Value` - an expression returning a `Constant<string>`
  - `Pattern` - `string` or an expression returning a `Constant<string>`

- `SwitchBooleanToDouble` - maps `1.0` to `true` and `0.0` to `false`.

- `GetContextItem` - gets an item from the expression-context.
  - `Key` - the key of the item to get. _Use `ExpressionContext.CreateKey` to create a key when testing._