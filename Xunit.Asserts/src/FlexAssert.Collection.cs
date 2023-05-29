using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Xunit.Sdk;

namespace Xunit;

public partial class FlexAssert
{
    /// <summary>
    /// Verifies that all items in the collection pass when executed against
    /// action.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="action">The action to test each item against.</param>
    /// <exception cref="AllException">Thrown when the collection contains at least one non-matching element.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert All<T>(IEnumerable<T> collection, Action<T> action)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        return this.All(collection, (item, _) => action(item));
    }

    /// <summary>
    /// Verifies that all items in the collection pass when executed against
    /// action. The item index is provided to the action, in addition to the item.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="action">The action to test each item against.</param>
    /// <exception cref="AllException">Thrown when the collection contains at least one non-matching element.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert All<T>(IEnumerable<T> collection, Action<T, int> action)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var errors = new Stack<Tuple<int, object?, Exception>>();
        var idx = 0;

        foreach (var item in collection)
        {
            try
            {
                action(item, idx);
            }
            catch (Exception ex)
            {
                errors.Push(new Tuple<int, object?, Exception>(idx, item, ex));
            }

            ++idx;
        }

        if (errors.Count > 0)
            throw new AllException(idx, errors.ToArray());

        return this;
    }

    /// <summary>
    /// Verifies that all items in the collection pass when executed against
    /// action.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="action">The action to test each item against.</param>
    /// <exception cref="AllException">Thrown when the collection contains at least one non-matching element.</exception>
    /// <returns>A value task.</returns>
    [SuppressMessage("", "S4457", Justification = "Match xunit's design")]
    [SuppressMessage("", "AsyncFixer01", Justification = "Match xunit's design")]
    public async ValueTask AllAsync<T>(IEnumerable<T> collection, Func<T, ValueTask> action)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        await this.AllAsync(collection, async (item, _) => await action(item).ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Verifies that all items in the collection pass when executed against
    /// action. The item index is provided to the action, in addition to the item.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="action">The action to test each item against.</param>
    /// <exception cref="AllException">Thrown when the collection contains at least one non-matching element.</exception>
    /// <returns>A value task.</returns>
    [SuppressMessage("", "S4457", Justification = "Match xunit's design")]
    public async ValueTask AllAsync<T>(IEnumerable<T> collection, Func<T, int, ValueTask> action)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var errors = new Stack<Tuple<int, object?, Exception>>();
        var idx = 0;

        foreach (var item in collection)
        {
            try
            {
                await action(item, idx).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errors.Push(new Tuple<int, object?, Exception>(idx, item, ex));
            }

            ++idx;
        }

        if (errors.Count > 0)
            throw new AllException(idx, errors.ToArray());
    }

    /// <summary>
    /// Verifies that a collection contains exactly a given number of elements, which meet
    /// the criteria provided by the element inspectors.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection to be inspected.</param>
    /// <param name="elementInspectors">The element inspectors, which inspect each element in turn. The
    /// total number of element inspectors must exactly match the number of elements in the collection.</param>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Collection<T>(IEnumerable<T> collection, params Action<T>[] elementInspectors)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (elementInspectors is null)
            throw new ArgumentNullException(nameof(elementInspectors));

        var elements = collection.ToArray();
        var expectedCount = elementInspectors.Length;
        var actualCount = elements.Length;

        if (expectedCount != actualCount)
            throw new CollectionException(collection, expectedCount, actualCount);

        for (var idx = 0; idx < actualCount; idx++)
        {
            try
            {
                elementInspectors[idx](elements[idx]);
            }
            catch (Exception ex)
            {
                throw new CollectionException(collection, expectedCount, actualCount, idx, ex);
            }
        }

        return this;
    }

    /// <summary>
    /// Verifies that a collection contains exactly a given number of elements, which meet
    /// the criteria provided by the element inspectors.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection to be inspected.</param>
    /// <param name="elementInspectors">The element inspectors, which inspect each element in turn. The
    /// total number of element inspectors must exactly match the number of elements in the collection.</param>
    /// <returns>A value task.</returns>
    [SuppressMessage("", "S4457", Justification = "Match xunit's design")]
    public async ValueTask CollectionAsync<T>(IEnumerable<T> collection, params Func<T, ValueTask>[] elementInspectors)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (elementInspectors is null)
            throw new ArgumentNullException(nameof(elementInspectors));

        var elements = collection.ToArray();
        var expectedCount = elementInspectors.Length;
        var actualCount = elements.Length;

        if (expectedCount != actualCount)
            throw new CollectionException(collection, expectedCount, actualCount);

        for (var idx = 0; idx < actualCount; idx++)
        {
            try
            {
                await elementInspectors[idx](elements[idx]).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new CollectionException(collection, expectedCount, actualCount, idx, ex);
            }
        }
    }

    /// <summary>
    /// Verifies that a collection contains a given object.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="expected">The object expected to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <exception cref="ContainsException">Thrown when the object is not present in the collection.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Contains<T>(T expected, IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        // If an equality comparer is not explicitly provided, call into ICollection<T>.Contains which may
        // use the collection's equality comparer for types like HashSet and Dictionary.
        var iCollection = collection as ICollection<T>;
        if (iCollection?.Contains(expected) == true)
            return this;

        // We don't throw if either ICollection<T>.Contains or our custom equality comparer says the collection
        // has the item.
        return this.Contains(expected, collection, new AssertEqualityComparer<T>());
    }

    /// <summary>
    /// Verifies that a collection contains a given object, using an equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="expected">The object expected to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <param name="comparer">The comparer used to equate objects in the collection with the expected object.</param>
    /// <exception cref="ContainsException">Thrown when the object is not present in the collection.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (comparer is null)
            throw new ArgumentNullException(nameof(comparer));

        if (collection.Contains(expected, comparer))
            return this;

        throw new ContainsException(expected, collection);
    }

    /// <summary>
    /// Verifies that a collection contains a given object.
    /// </summary>
    /// <typeparam name="T">The type of the object to be verified.</typeparam>
    /// <param name="collection">The collection to be inspected.</param>
    /// <param name="filter">The filter used to find the item you're ensuring the collection contains.</param>
    /// <exception cref="ContainsException">Thrown when the object is not present in the collection.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Contains<T>(IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        foreach (var item in collection)
        {
            if (filter(item))
                return this;
        }

        throw new ContainsException("(filter expression)", collection);
    }

    /// <summary>
    /// Verifies that a dictionary contains a given key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys of the object to be verified.</typeparam>
    /// <typeparam name="TValue">The type of the values of the object to be verified.</typeparam>
    /// <param name="expected">The object expected to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <returns>The value associated with <paramref name="expected"/>.</returns>
    /// <exception cref="ContainsException">Thrown when the object is not present in the collection.</exception>
    public TValue Contains<TKey, TValue>(TKey expected, IReadOnlyDictionary<TKey, TValue> collection)
        where TKey : notnull
    {
        if (expected is null)
            throw new ArgumentNullException(nameof(expected));

        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (!collection.TryGetValue(expected, out var value))
            throw new ContainsException(expected, collection.Keys);

        return value;
    }

    /// <summary>
    /// Verifies that a dictionary contains a given key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys of the object to be verified.</typeparam>
    /// <typeparam name="TValue">The type of the values of the object to be verified.</typeparam>
    /// <param name="expected">The object expected to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <returns>The value associated with <paramref name="expected"/>.</returns>
    /// <exception cref="ContainsException">Thrown when the object is not present in the collection.</exception>
    public TValue Contains<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> collection)
        where TKey : notnull
    {
        if (expected is null)
            throw new ArgumentNullException(nameof(expected));

        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (!collection.TryGetValue(expected, out var value))
            throw new ContainsException(expected, collection.Keys);

        return value;
    }

    /// <summary>
    /// Verifies that a collection does not contain a given object.
    /// </summary>
    /// <typeparam name="T">The type of the object to be compared.</typeparam>
    /// <param name="expected">The object that is expected not to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert DoesNotContain<T>(T expected, IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        // If an equality comparer is not explicitly provided, call into ICollection<T>.Contains which may
        // use the collection's equality comparer for types like HashSet and Dictionary.
        var iCollection = collection as ICollection<T>;
        if (iCollection?.Contains(expected) == true)
            throw new DoesNotContainException(expected, collection);

        // We don't throw only if both ICollection<T>.Contains and our custom equality comparer say the collection
        // doesn't have the item.
        return this.DoesNotContain(expected, collection, new AssertEqualityComparer<T>());
    }

    /// <summary>
    /// Verifies that a collection does not contain a given object, using an equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of the object to be compared.</typeparam>
    /// <param name="expected">The object that is expected not to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <param name="comparer">The comparer used to equate objects in the collection with the expected object.</param>
    /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert DoesNotContain<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (comparer is null)
            throw new ArgumentNullException(nameof(comparer));

        if (!collection.Contains(expected, comparer))
            return this;

        throw new DoesNotContainException(expected, collection);
    }

    /// <summary>
    /// Verifies that a collection does not contain a given object.
    /// </summary>
    /// <typeparam name="T">The type of the object to be compared.</typeparam>
    /// <param name="collection">The collection to be inspected.</param>
    /// <param name="filter">The filter used to find the item you're ensuring the collection does not contain.</param>
    /// <exception cref="DoesNotContainException">Thrown when the object is present inside the container.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert DoesNotContain<T>(IEnumerable<T> collection, Predicate<T> filter)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        foreach (var item in collection)
        {
            if (filter(item))
                throw new DoesNotContainException("(filter expression)", collection);
        }

        return this;
    }

    /// <summary>
    /// Verifies that a dictionary does not contain a given key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys of the object to be verified.</typeparam>
    /// <typeparam name="TValue">The type of the values of the object to be verified.</typeparam>
    /// <param name="expected">The object expected to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <exception cref="DoesNotContainException">Thrown when the object is present in the collection.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert DoesNotContain<TKey, TValue>(TKey expected, IReadOnlyDictionary<TKey, TValue> collection)
        where TKey : notnull
    {
        if (expected is null)
            throw new ArgumentNullException(nameof(expected));

        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        return this.DoesNotContain(expected, collection.Keys);
    }

    /// <summary>
    /// Verifies that a dictionary does not contain a given key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys of the object to be verified.</typeparam>
    /// <typeparam name="TValue">The type of the values of the object to be verified.</typeparam>
    /// <param name="expected">The object expected to be in the collection.</param>
    /// <param name="collection">The collection to be inspected.</param>
    /// <exception cref="DoesNotContainException">Thrown when the object is present in the collection.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert DoesNotContain<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> collection)
        where TKey : notnull
    {
        if (expected is null)
            throw new ArgumentNullException(nameof(expected));

        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        return this.DoesNotContain(expected, collection.Keys);
    }

    /// <summary>
    /// Verifies that a collection is empty.
    /// </summary>
    /// <param name="collection">The collection to be inspected.</param>
    /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
    /// <exception cref="EmptyException">Thrown when the collection is not empty.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Empty(IEnumerable collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        var enumerator = collection.GetEnumerator();
        try
        {
            if (enumerator.MoveNext())
                throw new EmptyException(collection);
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        return this;
    }

    /// <summary>
    /// Verifies that two sequences are equivalent, using a default comparer.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be compared.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The value to be compared against.</param>
    /// <exception cref="EqualException">Thrown when the objects are not equal.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        return this.Equal(expected, actual, GetEqualityComparer<T>());
    }

    /// <summary>
    /// Verifies that two sequences are equivalent, using a custom equatable comparer.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be compared.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The value to be compared against.</param>
    /// <param name="comparer">The comparer used to compare the two objects.</param>
    /// <exception cref="EqualException">Thrown when the objects are not equal.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
    {
        return this.Equal(expected, actual, GetEqualityComparer<T>());
    }

    /// <summary>
    /// Verifies that a collection is not empty.
    /// </summary>
    /// <param name="collection">The collection to be inspected.</param>
    /// <exception cref="ArgumentNullException">Thrown when a null collection is passed.</exception>
    /// <exception cref="NotEmptyException">Thrown when the collection is empty.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert NotEmpty(IEnumerable collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        var enumerator = collection.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
                throw new NotEmptyException();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        return this;
    }

    /// <summary>
    /// Verifies that two sequences are not equivalent, using a default comparer.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be compared.</typeparam>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <exception cref="NotEqualException">Thrown when the objects are equal.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert NotEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        return this.NotEqual(expected, actual, GetEqualityComparer<T>());
    }

    /// <summary>
    /// Verifies that two sequences are not equivalent, using a custom equality comparer.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be compared.</typeparam>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <param name="comparer">The comparer used to compare the two objects.</param>
    /// <exception cref="NotEqualException">Thrown when the objects are equal.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert NotEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
    {
        return this.NotEqual(expected, actual, GetEqualityComparer<T>());
    }

    /// <summary>
    /// Verifies that the given collection contains only a single
    /// element of the given type.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>The single item in the collection.</returns>
    /// <exception cref="SingleException">Thrown when the collection does not contain
    /// exactly one element.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public object? Single(IEnumerable collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        return this.Single(collection.Cast<object>());
    }

    /// <summary>
    /// Verifies that the given collection contains only a single
    /// element of the given value. The collection may or may not
    /// contain other values.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="expected">The value to find in the collection.</param>
    /// <returns>The single item in the collection.</returns>
    /// <exception cref="SingleException">Thrown when the collection does not contain
    /// exactly one element.</exception>
    /// <returns>An instance of <see cref="IAssert" /> for fluent chaining.</returns>
    public IAssert Single(IEnumerable collection, [AllowNull] object expected)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        GetSingleResult(collection.Cast<object>(), item => object.Equals(item, expected), ArgumentFormatter.Format(expected));

        return this;
    }

    /// <summary>
    /// Verifies that the given collection contains only a single
    /// element of the given type.
    /// </summary>
    /// <typeparam name="T">The collection type.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <returns>The single item in the collection.</returns>
    /// <exception cref="SingleException">Thrown when the collection does not contain
    /// exactly one element.</exception>
    public T Single<T>(IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        return GetSingleResult(collection, null, null);
    }

    /// <summary>
    /// Verifies that the given collection contains only a single
    /// element of the given type which matches the given predicate. The
    /// collection may or may not contain other values which do not
    /// match the given predicate.
    /// </summary>
    /// <typeparam name="T">The collection type.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="predicate">The item matching predicate.</param>
    /// <returns>The single item in the filtered collection.</returns>
    /// <exception cref="SingleException">Thrown when the filtered collection does
    /// not contain exactly one element.</exception>
    public T Single<T>(IEnumerable<T> collection, Predicate<T> predicate)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return GetSingleResult(collection, predicate, "(filter expression)");
    }

    private static T GetSingleResult<T>(IEnumerable<T> collection, Predicate<T>? predicate, string? expectedArgument)
    {
        var count = 0;
        var result = default(T);

        foreach (var item in collection)
        {
            if ((predicate == null || predicate(item)) && ++count == 1)
                result = item;
        }

        return count switch
        {
            0 => throw Single2Exception.Empty(expectedArgument),
            1 => result!,
            _ => throw Single2Exception.MoreThanOne(count, expectedArgument),
        };
    }
}