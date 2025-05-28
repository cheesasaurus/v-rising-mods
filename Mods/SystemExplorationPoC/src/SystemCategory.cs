namespace SystemExplorationPoC;

public enum SystemCategory
{
    Group, // is a ComponentSystemGroup
    Base, // is a ComponentSystemBase, but not a group
    Unmanaged, // implements ISystem
    Unknown, // we don't know the type of system
}