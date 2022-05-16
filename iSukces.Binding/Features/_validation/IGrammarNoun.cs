using System;

namespace iSukces.Binding
{
    public interface IGrammarNoun
    {
        string Choose(int number, params string[] forms);
    }

    public class PolishGrammarNoun : IGrammarNoun
    {
        public string Choose(int number, string[] forms)
        {
            if (forms is not { Length: 3 })
                throw new ArgumentException();
            number =  Math.Abs(number);
            number %= 100;
            if (number == 0)
                return forms[2];
            if (number == 1)
                return forms[0];
            if (number <= 4)
                return forms[1];
            if (number <= 21)
                return forms[2];

            number %= 10;
            if (number <= 1)
                return forms[2];

            if (number <= 4)
                return forms[1];
            return forms[2];
        }
    }
}
