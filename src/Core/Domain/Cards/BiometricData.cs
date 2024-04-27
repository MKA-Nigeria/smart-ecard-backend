namespace Domain.Cards;
public class BiometricData
{
    public byte[] Fingerprint { get; private set; }
    public byte[] RetinalScan { get; private set; }

    private BiometricData()
    {

    }

    public BiometricData(byte[] fingerprint, byte[] retinalScan)
    {
        Fingerprint = fingerprint;
        RetinalScan = retinalScan;
    }
}
