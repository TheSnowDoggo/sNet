namespace sNet.CScriptPro;

public interface IProperty
{
	public bool Serializable { get; }
	
	public Obj this[Part part] { get; set; }

	public int Serialize(Part part, Stream stream);
	public void Deserialize(Part part, Stream stream);
}