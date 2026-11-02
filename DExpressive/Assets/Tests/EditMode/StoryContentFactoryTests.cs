using NUnit.Framework;
using System;
using System.Reflection;

public class StoryContentFactoryTests
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    [TestCase(9)]
    [TestCase(10)]
    public void EveryDayHasAStartConversation(int day)
    {
        bool created = TryCreate("day" + day + "_start", out object node);

        Assert.That(created, Is.True);
        Assert.That(node, Is.Not.Null);
        Assert.That(Field<string>(node, "nodeId"), Is.EqualTo("day" + day + "_start"));
        Assert.That(Field<string>(node, "dialogueText"), Is.Not.Empty);
        Assert.That((Array)Field<object>(node, "choices"), Is.Not.Empty);
    }

    [Test]
    public void DayNineAndTenHaveTheEndingChoices()
    {
        TryCreate("day9_door", out object dayNineDoor);
        TryCreate("day10_door", out object dayTenDoor);

        Assert.That(FirstChoiceOption(dayNineDoor), Is.EqualTo("day9_open_door"));
        Assert.That(FirstChoiceOption(dayTenDoor), Is.EqualTo("day10_step_out"));
    }

    [Test]
    public void UnknownDaysAndInteractionsDoNotCreateDialogue()
    {
        Assert.That(TryCreate("day11_start", out _), Is.False);
        Assert.That(TryCreate("day1_unknown", out _), Is.False);
        Assert.That(TryCreate("not_a_day", out _), Is.False);
    }

    private static bool TryCreate(string nodeId, out object node)
    {
        Type factory = Type.GetType("StoryContentFactory, Assembly-CSharp");
        Assert.That(factory, Is.Not.Null, "게임 스토리 팩토리를 찾을 수 없습니다.");
        MethodInfo method = factory.GetMethod("TryCreate", BindingFlags.Public | BindingFlags.Static);
        object[] arguments = { nodeId, null };
        bool created = (bool)method.Invoke(null, arguments);
        node = arguments[1];
        return created;
    }

    private static string FirstChoiceOption(object node)
    {
        Array choices = (Array)Field<object>(node, "choices");
        return Field<string>(choices.GetValue(0), "optionId");
    }

    private static T Field<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance).GetValue(target);
    }
}
