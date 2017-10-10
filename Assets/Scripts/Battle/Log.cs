using System.Collections;
public class Log {
    public bool executed = false;
    public LogDisplay logDisplay;
    public virtual string GetText() {
        return "";
    }
    public virtual IEnumerator Execute() {
        yield return null;
    }
}
