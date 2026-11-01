using UnityEngine;

/// <summary>Provides the 10-day core scenario when a node is not authored in DialogueDatabase.asset.</summary>
public static class StoryContentFactory
{
    private static readonly string[] DayTitles =
    {
        "", "수레바퀴", "손상", "균열", "늪", "침잠",
        "낙하", "격돌", "혼란", "비상", "발걸음"
    };

    private static readonly string[] DayIntroductions =
    {
        "",
        "늦은 아침이다. 방은 정리되지 않았고, 휴대폰을 확인할 마음도 잘 나지 않는다.",
        "어제와 같은 방인데도 일상의 작은 행동들이 더 무겁게 느껴진다.",
        "무엇을 해야 하는지는 알지만, 간단한 일에도 이전보다 시간이 오래 걸린다.",
        "해야 할 일을 알고 있어도 몸이 쉽게 움직이지 않는다.",
        "작은 행동 하나조차 선택이 된다. 오늘은 거울이 유난히 신경 쓰인다.",
        "제대로 잠들지 못했다. 주변을 신경 쓸 여유가 줄어든다.",
        "혼자 감당하기 어렵다는 생각이 든다. 누군가에게 연락할 수 있을까.",
        "어제의 대화가 자꾸 떠오른다. 후회와 안도감이 함께 남아 있다.",
        "문 앞에 선다. 오늘은 문고리를 잡은 손을 내리지 않을 수 있을지도 모른다.",
        "문을 열어 본 다음 날이다. 오늘은 방 밖으로 한 걸음 나갈 준비를 한다."
    };

    public static bool TryCreate(string nodeId, out DialogueNode node)
    {
        node = null;
        if (string.IsNullOrWhiteSpace(nodeId) || !nodeId.StartsWith("day")) return false;

        int separator = nodeId.IndexOf('_');
        if (separator < 4 || !int.TryParse(nodeId.Substring(3, separator - 3), out int day) || day < 1 || day > 10)
        {
            return false;
        }

        string interaction = nodeId.Substring(separator + 1);
        bool created = interaction switch
        {
            "start" => CreateStart(day, out node),
            "phone" => CreatePhone(day, out node),
            "meal" => CreateMeal(day, out node),
            "medicine" => CreateMedicine(day, out node),
            "window" => CreateWindow(day, out node),
            "door" => CreateDoor(day, out node),
            "mirror" => CreateMirror(day, out node),
            "shower" => CreateShower(day, out node),
            "tidy" => CreateTidy(day, out node),
            _ => false
        };
        if (created && node != null) node.nodeId = nodeId;
        return created;
    }

    private static bool CreateStart(int day, out DialogueNode node)
    {
        node = Create(day, "시작", DayTitles[day] + " - " + DayIntroductions[day],
            Choice("방을 둘러본다", "day" + day + "_begin", 0));
        return true;
    }

    private static bool CreatePhone(int day, out DialogueNode node)
    {
        if (day == 7)
        {
            node = Create(day, "휴대폰", "누구에게 연락할까? 혼자 버티기 어렵다는 생각이 든다.",
                Choice("오랜 친구에게 연락한다", "day7_contact_friend", 8),
                Choice("부모님에게 연락한다", "day7_contact_parent", 5));
            return true;
        }
        if (day == 8)
        {
            node = Create(day, "휴대폰", "어제의 대화가 떠오른다. 답장을 보낼 수 있을까?",
                Choice("조금 더 이야기한다", "day8_reply_more", 6),
                Choice("짧게 답한다", "day8_reply_short", 2));
            return true;
        }
        if (day >= 9)
        {
            node = Create(day, "휴대폰", "읽지 않은 연락이 남아 있다. 답장은 늦어도 괜찮다.",
                Choice("짧게 안부를 보낸다", "day" + day + "_reply", 3),
                Choice("화면을 덮는다", "day" + day + "_phone_rest", 0));
            return true;
        }

        string message = day switch
        {
            1 => "부재중 전화와 짧은 메시지가 쌓여 있다. 답장할 수 있을까?",
            2 => "반 친구에게서 메시지가 왔다. '뭐해?'",
            3 => "어제 확인하지 못한 연락이 아직 남아 있다.",
            4 => "답장하지 못한 문장이 화면 위에 남아 있다.",
            5 => "알림창을 바라보다가 화면을 끈다.",
            6 => "계속 피했던 연락이다. 짧게라도 답장할 수 있을까?",
            _ => "휴대폰이 조용히 진동한다."
        };
        node = Create(day, "휴대폰", message,
            Choice("짧게 답장한다", "day" + day + "_reply", day >= 6 ? 4 : 2),
            Choice("나중에 답장한다", "day" + day + "_reply_later", day >= 4 ? -3 : -1));
        return true;
    }

    private static bool CreateMeal(int day, out DialogueNode node)
    {
        node = Create(day, "식사", day >= 6 ? "무언가를 먹을 힘이 남아 있을까." : "먹을 것을 꺼내 본다.",
            Choice("조금 먹는다", "day" + day + "_meal", day >= 6 ? 3 : 7),
            Choice("그냥 둔다", "day" + day + "_meal_skip", day >= 6 ? -8 : -5));
        return true;
    }

    private static bool CreateMedicine(int day, out DialogueNode node)
    {
        node = Create(day, "약", "약봉투가 눈에 들어온다. 복용 여부는 오늘의 선택으로 남는다.",
            Choice("약을 복용한다", "day" + day + "_medicine", 3),
            Choice("다시 넣어 둔다", "day" + day + "_medicine_skip", -3));
        return true;
    }

    private static bool CreateWindow(int day, out DialogueNode node)
    {
        string text = day >= 9 ? "밖의 빛이 방 안으로 들어온다. 세상은 계속 움직이고 있다." : "창밖을 본다. 풍경은 같은데 마음은 매일 다르다.";
        node = Create(day, "창문", text,
            Choice(day >= 8 ? "커튼을 조금 연다" : "창밖을 더 본다", "day" + day + "_window", day >= 8 ? 3 : 0),
            Choice("커튼을 닫는다", "day" + day + "_window_close", -1));
        return true;
    }

    private static bool CreateDoor(int day, out DialogueNode node)
    {
        if (day == 9)
        {
            node = Create(day, "현관문", "문고리를 잡는다. 이번에는 손을 내리지 않는다.",
                Choice("문을 연다", "day9_open_door", 8), Choice("잠시 숨을 고른다", "day9_door_pause", 1));
            return true;
        }
        if (day == 10)
        {
            node = Create(day, "현관문", "신발을 신고 문 앞에 선다. 밖으로 한 걸음 나갈 준비가 되었다.",
                Choice("밖으로 한 걸음 나간다", "day10_step_out", 15), Choice("문 앞에서 숨을 고른다", "day10_breathe", 3));
            return true;
        }
        node = Create(day, "현관문", "문 앞까지 왔지만, 지금은 손을 뻗기 어렵다.",
            Choice("문고리에서 손을 뗀다", "day" + day + "_door_wait", 0));
        return true;
    }

    private static bool CreateMirror(int day, out DialogueNode node)
    {
        node = Create(day, "거울", day >= 6 ? "거울 속 얼굴을 한동안 바라본다. 언제부터 이랬을까." : "거울에 비친 자신을 잠시 바라본다.",
            Choice("오늘은 그냥 쉬자", "day" + day + "_mirror_rest", 1),
            Choice("스스로를 다그친다", "day" + day + "_mirror_blame", -6));
        return true;
    }

    private static bool CreateShower(int day, out DialogueNode node)
    {
        node = Create(day, "세면대", "물을 틀어 보지만 한동안 멍하니 바라본다.",
            Choice("세수한다", "day" + day + "_shower", 4), Choice("조금 있다가 하기로 한다", "day" + day + "_shower_skip", -2));
        return true;
    }

    private static bool CreateTidy(int day, out DialogueNode node)
    {
        node = Create(day, "정리", "책상 위에 빈 컵과 쓰레기가 쌓여 있다.",
            Choice("하나라도 치운다", "day" + day + "_tidy", 4), Choice("그냥 둔다", "day" + day + "_tidy_skip", -1));
        return true;
    }

    private static DialogueNode Create(int day, string speaker, string text, params DialogueChoice[] choices)
    {
        return new DialogueNode { nodeId = "day" + day + "_generated", speakerName = speaker, dialogueText = text, choices = choices };
    }

    private static DialogueChoice Choice(string text, string optionId, int mentalChange)
    {
        return new DialogueChoice { optionId = optionId, buttonText = text, mentalChange = mentalChange, nextStoryId = optionId };
    }
}
