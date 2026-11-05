using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName="DExpressive/Additional room props")]
public class AdditionalRoomPropsPalette : ScriptableObject
{
    public GameObject phone,meal,shoe,fridge,vanity,mirror,toilet,paper,brush,shower,tub,towelRail,soap,bathMat,trashBin,shelf,washingMachine;

    public void Apply(Transform room)
    {
        if(new[]{phone,meal,shoe,fridge,vanity,mirror,toilet,paper,brush,shower,tub,towelRail,soap,bathMat,shelf,washingMachine}.Any(p=>p==null))
        { Debug.LogError("Additional room props palette is incomplete; original props retained.");return; }
        var root=new GameObject("Additional Room Props").transform;root.SetParent(room,false);
        // The old isolated jamb stood in the middle of the entrance. Keep it at the partition edge.
        var jamb=room.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Bathroom Door Frame");
        jamb.position=new Vector3(-1.57f,1.2f,-0.13f);
        Replace(room,root,"Phone - Click to Read",phone,new Vector3(2.5f,1.145f,5.5f),0,PrototypeInteractionTarget.InteractionType.Phone);
        Replace(room,root,"Meal - Interact",meal,new Vector3(1.75f,1.145f,5.5f),-12,PrototypeInteractionTarget.InteractionType.Meal);
        Replace(room,root,"Sink - Click to Wash",vanity,new Vector3(-3.25f,0.25f,1.1f),180,PrototypeInteractionTarget.InteractionType.Shower);
        Replace(room,root,"Mirror - Click to Look",mirror,new Vector3(-3.25f,1.725f,1.32f),180,PrototypeInteractionTarget.InteractionType.Mirror);
        Replace(room,root,"Bathroom Shelf",shelf,new Vector3(-4.35f,1.44f,1.15f),0,null);
        Transform oldShoes=room.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Shoes");
        oldShoes.name="Legacy Placeholder - Shoes";oldShoes.gameObject.SetActive(false);
        var pair=new GameObject("Shoes").transform;pair.SetParent(root,false);
        Place(shoe,pair,"Shoe Left",new Vector3(3.43f,0.005f,-0.6f),-8);
        Place(shoe,pair,"Shoe Right",new Vector3(3.67f,0.005f,-0.6f),8);
        Place(fridge,root,"Refrigerator",new Vector3(4.42f,0,-0.2f),90);
        Place(washingMachine,root,"Washing Machine",new Vector3(4.42f,0,0.85f),-90);
        if(trashBin!=null)
        {
            var bin=Place(trashBin,root,"Trash Bin - Tidy",new Vector3(2.4f,0,-1.05f),0);
            bin.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Tidy);
            var contents=bin.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Object_4");
            if(contents!=null)contents.name="Trash Bin Contents";
        }
        Place(toilet,root,"Bathroom Toilet",new Vector3(-4.55f,0.2f,0.55f),90);
        Place(paper,root,"Toilet Paper Holder",new Vector3(-4.8f,0.82f,-0.03f),90);
        Place(brush,root,"Toilet Brush",new Vector3(-4.2f,0.05f,0.9f),0);
        var showerObject=Place(shower,root,"Bathroom Shower - Wash",new Vector3(-2.32f,0.05f,-0.78f),0);
        showerObject.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Shower);
        var bathtub=Place(tub,root,"Bathroom Bathtub - Wash",new Vector3(-3.95f,0.05f,-0.8f),90);
        bathtub.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Shower);
        Place(towelRail,root,"Bathroom Towel Rail",new Vector3(-2.25f,1.15f,1.32f),90);
        Place(soap,root,"Bathroom Hand Soap",new Vector3(-3.65f,0.92f,1),180);
        Place(bathMat,root,"Bathroom Bath Mat",new Vector3(-3.2f,0.052f,0.5f),0);
        Debug.Log("ADDITIONAL_ROOM_PROPS_APPLIED");
    }

    private static GameObject Place(GameObject prefab,Transform root,string name,Vector3 position,float yaw)
    {
        var instance=Instantiate(prefab,position,Quaternion.Euler(0,yaw,0),root);instance.name=name;return instance;
    }
    private static void Replace(Transform room,Transform root,string name,GameObject prefab,Vector3 position,float yaw,PrototypeInteractionTarget.InteractionType? type)
    {
        var original=room.GetComponentsInChildren<Transform>(true).First(t=>t.name==name);
        // Inactive placeholders must not be mistaken for the replacement when daily consumables are restored.
        original.name="Legacy Placeholder - "+name;original.gameObject.SetActive(false);
        var instance=Place(prefab,root,name,position,yaw);
        if(type.HasValue)instance.AddComponent<PrototypeInteractionTarget>().SetType(type.Value);
    }
}
