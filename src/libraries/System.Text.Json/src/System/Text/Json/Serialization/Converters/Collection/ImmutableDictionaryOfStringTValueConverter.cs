﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace System.Text.Json.Serialization.Converters
{
    internal sealed class ImmutableDictionaryOfStringTValueConverter<TCollection, TValue>
        : DictionaryDefaultConverter<TCollection, TValue>
        where TCollection : IReadOnlyDictionary<string, TValue>
    {
        protected override void Add(TValue value, JsonSerializerOptions options, ref ReadStack state)
        {
            string key = state.Current.JsonPropertyNameAsString!;
            ((Dictionary<string, TValue>)state.Current.ReturnValue!)[key] = value;
        }

        internal override bool CanHaveIdMetadata => false;

        protected override void CreateCollection(ref Utf8JsonReader reader, ref ReadStack state)
        {
            state.Current.ReturnValue = new Dictionary<string, TValue>();
        }

        protected override void ConvertCollection(ref ReadStack state, JsonSerializerOptions options)
        {
            JsonClassInfo classInfo = state.Current.JsonClassInfo;

            Func<IEnumerable<KeyValuePair<string, TValue>>, TCollection>? creator = (Func<IEnumerable<KeyValuePair<string, TValue>>, TCollection>?)classInfo.CreateObjectWithArgs;
            if (creator == null)
            {
                creator = options.MemberAccessorStrategy.CreateImmutableDictionaryCreateRangeDelegate<TValue, TCollection>();
                classInfo.CreateObjectWithArgs = creator;
            }

            state.Current.ReturnValue = creator((Dictionary<string, TValue>)state.Current.ReturnValue!);
        }

        protected internal override bool OnWriteResume(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options, ref WriteStack state)
        {
            IEnumerator<KeyValuePair<string, TValue>> enumerator;
            if (state.Current.CollectionEnumerator == null)
            {
                enumerator = value.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return true;
                }
            }
            else
            {
                enumerator = (IEnumerator<KeyValuePair<string, TValue>>)state.Current.CollectionEnumerator;
            }

            JsonConverter<TValue> converter = GetValueConverter(ref state);
            do
            {
                if (ShouldFlush(writer, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                if (state.Current.PropertyState < StackFramePropertyState.Name)
                {
                    state.Current.PropertyState = StackFramePropertyState.Name;
                    string key = GetKeyName(enumerator.Current.Key, ref state, options);
                    writer.WritePropertyName(key);
                }

                TValue element = enumerator.Current.Value;
                if (!converter.TryWrite(writer, element, options, ref state))
                {
                    state.Current.CollectionEnumerator = enumerator;
                    return false;
                }

                state.Current.EndDictionaryElement();
            } while (enumerator.MoveNext());

            return true;
        }
    }
}
