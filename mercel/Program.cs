using System;
using System.Linq;
using System.Security.Cryptography;

namespace mercel
{
    class Program
    { 
        const int MessageCount = 16;
        const int DigestSize = 32;
        static void Main(string[] args)
        {
            // Генерируем 16 уникальных сообщений для подписи
            byte[][] messages = GenerateMessages(MessageCount);

            // Вычисляем корень дерева Меркле
            byte[] merkleRoot = CalculateMerkleRoot(messages);

            // Подписываем сообщение
            byte[] signature = SignMessage(messages[0], merkleRoot);

            // Проверяем подпись
            bool isSignatureValid = VerifySignature(messages[0], signature, merkleRoot);

            // Выводим результаты
            Console.WriteLine("Merkle root: " + BitConverter.ToString(merkleRoot).Replace("-", ""));
            Console.WriteLine("Signature: " + BitConverter.ToString(signature).Replace("-", ""));
            Console.WriteLine("Is signature valid: " + isSignatureValid);
            Console.ReadKey();
        }
        static byte[][] GenerateMessages(int count)
        {
            byte[][] messages = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                messages[i] = BitConverter.GetBytes(i);
            }
            return messages;
        }

        //В функции `CalculateMerkleRoot` вычисляется корень дерева Меркле для заданного набора сообщений.  
        // Сначала создаются листовые узлы для каждого сообщения, затем создаются внутренние узлы, путем вычисления хэш-сумм двух дочерних узлов.
        //Затем в цикле повторяется процесс создания внутренних узлов, пока не будет создан единственный узел - корень дерева Меркле. 
        static byte[] CalculateMerkleRoot(byte[][] messages)
        {
            MerkleNode[] nodes = new MerkleNode[2 * MessageCount - 1];
            int nodeCount = MessageCount;

            // Leaf nodes
            for (int i = 0; i < MessageCount; i++)
            {
                nodes[i] = new MerkleNode(messages[i]);
            }

            // Inner nodes
            while (nodeCount > 1)
            {
                for (int i = 0; i < nodeCount / 2; i++)
                {
                    MerkleNode left = nodes[2 * i];
                    MerkleNode right = nodes[2 * i + 1];
                    byte[] hash = CalculateHash(left.Digest.Concat(right.Digest).ToArray());
                    nodes[nodeCount + i] = new MerkleNode(hash);
                }
                if (nodeCount % 2 == 1)
                {
                    nodes[nodeCount - 1] = new MerkleNode(nodes[2 * (nodeCount / 2)].Digest);
                }
                nodeCount = (nodeCount + 1) / 2;
            }

            return nodes[0].Digest;
        }

        static byte[] CalculateHash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }
        //Функция `SignMessage` используется для подписания сообщения на основе корня дерева Меркле.  
        //В этой функции объединяются сообщение и дайджест корня дерева Меркле, и затем 
        //вычисляется хэш-сумма для этого объединенного значения. Подпись представляет собой первые 32 байта этой хэш-суммы. 
        static byte[] SignMessage(byte[] message, byte[] merkleRoot)
        {
            byte[] signature = new byte[DigestSize];
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = message.Concat(merkleRoot).ToArray();
                byte[] hash = sha256.ComputeHash(data);
                Array.Copy(hash, signature, DigestSize);
            }
            return signature;
        }
        //Функция `VerifySignature` используется для проверки подписи.
        //В этой функции сообщение подписывается снова, используя корень дерева Меркле, и вычисленная подпись сравнивается с оригинальной подписью.
        //Если они совпадают, подпись считается действительной.
        static bool VerifySignature(byte[] message, byte[] signature, byte[] merkleRoot)
        {
            byte[] expectedSignature = SignMessage(message, merkleRoot);
            return expectedSignature.SequenceEqual(signature);
        }
    }
    class MerkleNode
    {
        public byte[] Digest { get; }

        public MerkleNode(byte[] digest)
        {
            Digest = digest;
        }
        //Класс `MerkleNode` представляет узел дерева Меркле. Узел может быть как листовым, так и внутренним. 
        // Каждый узел содержит хэш-сумму (дайджест) сообщения или внутренних узлов дерева. 
        //Листовые узлы содержат дайджесты сообщений, а внутренние узлы содержат дайджесты, вычисленные из дайджестов двух дочерних узлов. 
    }

}
