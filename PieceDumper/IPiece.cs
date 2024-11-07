using System;
using UnityEngine;

/**
 * Provides an interface for us
 * to avoid reflections, since we really
 * only want a few methods
 */
public interface IPiece
{
    int GetInstanceID();
    Type WrappedType();
    string m_name
    {
        get;
    }

    string name
    {
        get;
    }
}

/**
 * Wrap around pieces so that
 * we can obtain the methods we need
 * without using reflections
 */
public class PieceWrapper<T> : IPiece where T : Component
{
    private readonly T _piece;
    public PieceWrapper(T piece) => _piece = piece;
    public int GetInstanceID() => _piece.GetInstanceID();
    public string m_name => (string)_piece.GetType().GetField("m_name")?.GetValue(_piece);
    public Type WrappedType() => _piece.GetType();
    public string name => _piece.name;
}
