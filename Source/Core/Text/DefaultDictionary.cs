namespace Diary.Core.Text
{
    internal class DefaultDictionary
    {
        private static DefaultDictionary _instance;
        private TokensDictionary _dictionary;

        public TokensDictionary Dictionary
        {
            get { return _dictionary; }
        }

        private DefaultDictionary()
        {
            _dictionary = new TokensDictionary
            {
                {
                    "NAME",
                    new TokenTranslation()
                    {
                        {
                            "ANIMAL",
                            new RandomString(
                                "lion",
                                "bee",
                                "bat",
                                "cow",
                                "dog",
                                "cat",
                                "duck",
                                "snake",
                                "badger",
                                "bear",
                                "bull",
                                "bird",
                                "butterfly",
                                "calf",
                                "cheetah",
                                "chicken",
                                "crab",
                                "deer",
                                "donkey",
                                "eagle",
                                "elephant",
                                "fish"
                            )
                        },
                        { "FIGHT", new RandomString("fight", "combat", "brawl", "wrestle") }
                    }
                },
                {
                    "ADJ",
                    new TokenTranslation()
                    {
                        {
                            "DEATH_QUALIFICATION",
                            new RandomString(
                                "horrible",
                                "lethal",
                                "fatal",
                                "harmful",
                                "harmless",
                                "tough",
                                "severe",
                                "brutal",
                                "rough",
                                "painful"
                            )
                        }
                    }
                },
                {
                    "ADV",
                    new TokenTranslation()
                    {
                        {
                            "DEATH_QUALIFICATION",
                            new RandomString("horribly", "harmfully", "badly", "wildly")
                        }
                    }
                }
            };
        }

        public static DefaultDictionary GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DefaultDictionary();
            }

            return _instance;
        }
    }
}
