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
        if (day >= 4 && day <= 8 && TryCreateExpandedDayInteraction(day, interaction, out node))
        {
            node.nodeId = nodeId;
            return true;
        }

        if (day == 3 && IsDayThreeRestInteraction(interaction))
        {
            node = Create(day, GetDayThreeRestSpeaker(interaction), "얼른 자고싶다",
                Choice("확인", "day3_" + interaction + "_skip", 0));
            node.nodeId = nodeId;
            return true;
        }

        bool created = interaction switch
        {
            "start" => CreateStart(day, out node),
            "sleep" => CreateSleep(day, out node),
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
        string firstAction = day == 1
            ? "아침이다. 핸드폰이나 확인해볼까"
            : "방을 둘러본다";
        node = Create(day, "시작", DayTitles[day] + " - " + DayIntroductions[day],
            Choice(firstAction, "day" + day + "_begin", 0));
        return true;
    }

    private static bool CreateSleep(int day, out DialogueNode node)
    {
        node = day == 10
            ? Create(day, "나", "아직 잘 수 없어. 오늘은 현관문 앞까지 가 보자.", Choice("현관문으로 간다", "day10_bed_to_door", 0))
            : null;
        return node != null;
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
            node = Create(day, "현관문", "문 앞에 섰지만, 아직 문고리를 돌릴 수 없다.",
                Choice("오늘은 여기까지", "day9_door_wait", 0));
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

    private static bool IsDayThreeRestInteraction(string interaction)
    {
        return interaction == "medicine" || interaction == "tidy" || interaction == "mirror" ||
               interaction == "window" || interaction == "door";
    }

    private static bool TryCreateExpandedDayInteraction(int day, string interaction, out DialogueNode node)
    {
        node = day switch
        {
            4 => CreateDayFourInteraction(interaction),
            5 => CreateDayFiveInteraction(interaction),
            6 => CreateDaySixInteraction(interaction),
            7 => CreateDaySevenInteraction(interaction),
            8 => CreateDayEightInteraction(interaction),
            _ => null
        };
        return node != null;
    }

    private static DialogueNode CreateDayFourInteraction(string interaction)
    {
        return interaction switch
        {
            "phone" => Create(4, "휴대폰", "답장을 작성하지만, 전송 버튼 앞에서 손이 멈춘다.",
                Choice("전송한다", "day4_phone_send", 1), Choice("지운다", "day4_phone_delete", -5)),
            "meal" => Create(4, "식사", "즉석밥을 전자레인지 앞에 두고 한동안 바라본다.",
                Choice("그래도 돌린다", "day4_meal_heat", 1), Choice("그냥 돌아간다", "day4_meal_leave", -5)),
            "medicine" => Create(4, "약", "약봉투를 손에 쥐지만, 뭘 해야 할지 모르겠다.",
                Choice("다시 넣어둔다", "day4_medicine_store", 0), Choice("버린다", "day4_medicine_discard", -3)),
            "shower" => Create(4, "세면대", "물을 틀고 세면대를 바라본다. 몸이 움직이지 않는다.",
                Choice("물을 끈다", "day4_shower_stop", 0)),
            "tidy" => Create(4, "청소", "컵과 쓰레기가 눈에 들어오지만 손을 뻗기 어렵다.",
                Choice("하나만 치운다", "day4_tidy_one", 1), Choice("그냥 둔다", "day4_tidy_skip", -1)),
            "mirror" => Create(4, "거울", "거울 속 얼굴을 보다가 시선을 피한다.",
                Choice("잠시 바라본다", "day4_mirror_look", 0), Choice("고개를 돌린다", "day4_mirror_avoid", -1)),
            "window" => Create(4, "창문", "커튼 너머의 빛이 낯설게 느껴진다.",
                Choice("커튼을 조금 연다", "day4_window_open", 1), Choice("그대로 둔다", "day4_window_leave", 0)),
            "door" => Create(4, "현관문", "문 밖의 택배가 떠오르지만 문고리까지 손이 가지 않는다.",
                Choice("문 앞에 서 본다", "day4_door_stand", 0), Choice("돌아선다", "day4_door_leave", -1)),
            _ => null
        };
    }

    private static DialogueNode CreateDayFiveInteraction(string interaction)
    {
        return interaction switch
        {
            "phone" => Create(5, "휴대폰", "알림창을 바라본다. 답장 하나가 너무 큰 일이 된다.",
                Choice("짧게 답장한다", "day5_phone_reply", 2), Choice("나중에 답한다", "day5_phone_later", -2)),
            "meal" => Create(5, "식사", "먹어야 한다는 걸 알지만 숟가락을 드는 것도 망설여진다.",
                Choice("한입 먹는다", "day5_meal_bite", 1), Choice("그냥 둔다", "day5_meal_skip", -5)),
            "medicine" => Create(5, "약", "약봉투가 눈에 들어오자 스스로가 더 작아지는 기분이 든다.",
                Choice("약을 챙긴다", "day5_medicine_take", 1), Choice("다시 넣어둔다", "day5_medicine_skip", -2)),
            "shower" => Create(5, "세면대", "물을 틀어도 거울을 마주할 자신이 없다.",
                Choice("세수한다", "day5_shower_wash", 2), Choice("물을 끈다", "day5_shower_skip", -2)),
            "tidy" => Create(5, "청소", "쓰레기 하나를 버리는 일도 오늘은 큰 선택처럼 느껴진다.",
                Choice("쓰레기 하나 버리기", "day5_tidy_one", 1), Choice("그냥 둔다", "day5_tidy_skip", -3)),
            "mirror" => Create(5, "거울", "왜 이것도 제대로 못 하지.",
                Choice("오늘은 그냥 쉬자", "day5_mirror_rest", 2), Choice("정신 차려야 한다", "day5_mirror_blame", -8)),
            "window" => Create(5, "창문", "방 안은 답답하지만 커튼을 여는 것도 망설여진다.",
                Choice("커튼을 조금 연다", "day5_window_open", 2), Choice("닫아 둔다", "day5_window_skip", -1)),
            "door" => Create(5, "현관문", "문 앞까지 가는 일도 너무 멀게 느껴진다.",
                Choice("문을 바라본다", "day5_door_look", 0), Choice("침대로 돌아간다", "day5_door_leave", -1)),
            _ => null
        };
    }

    private static DialogueNode CreateDaySixInteraction(string interaction)
    {
        return interaction switch
        {
            "phone" => Create(6, "휴대폰", "계속 피했던 연락이다. 짧게라도 답할 수 있을까.",
                Choice("짧게 답장한다", "day6_phone_reply", 3), Choice("화면을 끈다", "day6_phone_skip", -6)),
            "meal" => Create(6, "식사", "속은 비어 있는데 먹을 힘이 남아 있는지 모르겠다.",
                Choice("뭔가 먹는다", "day6_meal_eat", 1), Choice("그냥 둔다", "day6_meal_skip", -6)),
            "medicine" => Create(6, "약", "약봉투를 바라본다. 오늘은 판단할 여유도 없다.",
                Choice("약을 챙긴다", "day6_medicine_take", 1), Choice("다시 넣어둔다", "day6_medicine_skip", -3)),
            "shower" => Create(6, "세면대", "화장실까지 왔지만 어지러움에 손을 짚는다.",
                Choice("얼굴에 물을 묻힌다", "day6_shower_wash", 1), Choice("잠시 쉰다", "day6_shower_skip", -2)),
            "tidy" => Create(6, "청소", "주변을 정리할 여유는 없지만, 어지러운 방이 더 답답하다.",
                Choice("컵 하나 치운다", "day6_tidy_one", 1), Choice("그냥 둔다", "day6_tidy_skip", -2)),
            "mirror" => Create(6, "거울", "언제부터 이랬지.",
                Choice("잠시 바라본다", "day6_mirror_look", 0), Choice("시선을 피한다", "day6_mirror_avoid", -2)),
            "window" => Create(6, "창문", "바깥 소리가 멀게만 들린다.",
                Choice("빛을 본다", "day6_window_look", 0), Choice("커튼을 닫는다", "day6_window_skip", -1)),
            "door" => Create(6, "현관문", "문 밖으로 나갈 생각을 하자 숨이 막힌다.",
                Choice("문 앞에 선다", "day6_door_stand", 0), Choice("돌아선다", "day6_door_leave", -2)),
            _ => null
        };
    }

    private static DialogueNode CreateDaySevenInteraction(string interaction)
    {
        return interaction switch
        {
            "phone" => Create(7, "휴대폰", "계속 혼자 감당하기는 어렵다. 누구에게 연락할까.",
                Choice("부모님에게 연락한다", "day7_phone_parent", 15), Choice("오랜 친구에게 연락한다", "day7_phone_friend", 15)),
            "meal" => Create(7, "식사", "누군가에게 연락할 생각을 하니 조금은 숨이 돌아오는 것 같다.",
                Choice("조금 먹는다", "day7_meal_eat", 3), Choice("나중에 먹는다", "day7_meal_skip", -2)),
            "medicine" => Create(7, "약", "약봉투를 쥐고 한참 앉아 있다.",
                Choice("약을 챙긴다", "day7_medicine_take", 2), Choice("다시 넣어둔다", "day7_medicine_skip", -2)),
            "shower" => Create(7, "세면대", "찬물이 손끝에 닿자 아주 조금 정신이 든다.",
                Choice("세수한다", "day7_shower_wash", 3), Choice("물을 끈다", "day7_shower_skip", -1)),
            "tidy" => Create(7, "청소", "책상 위의 컵을 보며 오래 미뤄 둔 일들을 떠올린다.",
                Choice("컵을 치운다", "day7_tidy_one", 2), Choice("그대로 둔다", "day7_tidy_skip", -1)),
            "mirror" => Create(7, "거울", "혼자 견디기 힘들다는 말을 떠올린다.",
                Choice("숨을 고른다", "day7_mirror_breathe", 1), Choice("고개를 숙인다", "day7_mirror_avoid", -1)),
            "window" => Create(7, "창문", "밖은 평소처럼 움직이고 있다.",
                Choice("창밖을 본다", "day7_window_look", 1), Choice("커튼을 닫는다", "day7_window_skip", 0)),
            "door" => Create(7, "현관문", "문고리를 바라보다 휴대폰을 다시 생각한다.",
                Choice("잠시 선다", "day7_door_stand", 0), Choice("돌아선다", "day7_door_leave", -1)),
            _ => null
        };
    }

    private static DialogueNode CreateDayEightInteraction(string interaction)
    {
        return interaction switch
        {
            "phone" => Create(8, "휴대폰", "어제 너무 많은 이야기를 한 건 아닐까. 후속 메시지가 와 있다.",
                Choice("조금 더 이야기한다", "day8_phone_more", 6), Choice("짧게 답한다", "day8_phone_short", 4)),
            "meal" => Create(8, "식사", "제대로 한 끼를 먹어 볼까. 여전히 쉽지는 않다.",
                Choice("한 끼 먹는다", "day8_meal_eat", 5), Choice("조금 있다가 먹는다", "day8_meal_skip", -2)),
            "medicine" => Create(8, "약", "어제의 대화가 떠오른다. 혼자 결정할 필요만은 없을지도 모른다.",
                Choice("약을 챙긴다", "day8_medicine_take", 2), Choice("다시 넣어둔다", "day8_medicine_skip", -1)),
            "shower" => Create(8, "세면대", "얼굴에 물을 묻히자 조금은 오늘로 돌아오는 기분이 든다.",
                Choice("세수한다", "day8_shower_wash", 3), Choice("물을 끈다", "day8_shower_skip", -1)),
            "tidy" => Create(8, "청소", "컵 하나라도 치우면 방이 아주 조금 달라질 것 같다.",
                Choice("컵을 치운다", "day8_tidy_one", 3), Choice("그대로 둔다", "day8_tidy_skip", -1)),
            "mirror" => Create(8, "거울", "어제의 말을 떠올리면 아직 부끄럽지만, 아주 조금 안심도 된다.",
                Choice("거울을 본다", "day8_mirror_look", 1), Choice("시선을 피한다", "day8_mirror_avoid", -1)),
            "window" => Create(8, "창문", "커튼을 조금 열면 빛이 방 안으로 들어온다.",
                Choice("커튼을 조금 연다", "day8_window_open", 4), Choice("그대로 둔다", "day8_window_skip", -1)),
            "door" => Create(8, "현관문", "문 앞에서 어제의 대화를 떠올린다. 오늘은 아직 문을 열지 못한다.",
                Choice("문 앞에 선다", "day8_door_stand", 1), Choice("돌아선다", "day8_door_leave", -1)),
            _ => null
        };
    }

    private static string GetDayThreeRestSpeaker(string interaction)
    {
        return interaction switch
        {
            "medicine" => "약",
            "tidy" => "청소",
            "mirror" => "거울",
            "window" => "창문",
            "door" => "현관문",
            _ => ""
        };
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
