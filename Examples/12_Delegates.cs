/*
 * Copyright (c) 2009, Stefan Simek
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections;
using System.Text;
using TryAxis.RunSharp;

namespace TriAxis.RunSharp.Examples
{
	class _12_Delegates
	{
		// example based on the MSDN Delegates Sample (bookstore.cs)
		public static void GenBookstore(AssemblyGen ag)
		{
		    ITypeMapper m = ag.TypeMapper;
		    TypeGen book, processBookDelegate, BookDBLocal;

		    // A set of classes for handling a bookstore:
		    using (ag.Namespace("Bookstore"))
			{
				// Describes a book in the book list:
				book = ag.Public.Struct("Book");
				{
					FieldGen title = book.Public.Field(typeof(string), "Title");	   // Title of the book.
					FieldGen author = book.Public.Field(typeof(string), "Author");     // Author of the book.
					FieldGen price = book.Public.Field(typeof(decimal), "Price");      // Price of the book.
					FieldGen paperback = book.Public.Field(typeof(bool), "Paperback"); // Is it paperback?

					CodeGen g = book.Public.Constructor()
						.Parameter(typeof(string), "title")
						.Parameter(typeof(string), "author")
						.Parameter(typeof(decimal), "price")
						.Parameter(typeof(bool), "paperBack");
					{
						g.Assign(title, g.Arg("title"));
						g.Assign(author, g.Arg("author"));
						g.Assign(price, g.Arg("price"));
						g.Assign(paperback, g.Arg("paperBack"));
					}
				}

				// Declare a delegate type for processing a book:
				processBookDelegate = ag.Public.Delegate(typeof(void), "ProcessBookDelegate").Parameter(book, "book");

				// Maintains a book database.
				BookDBLocal = ag.Public.Class("BookDB");
				{
					// List of all books in the database:
					FieldGen list = BookDBLocal.Field(typeof(ArrayList), "list", Exp.New(typeof(ArrayList), m));

					// Add a book to the database:
					CodeGen g = BookDBLocal.Public.Method(typeof(void), "AddBook")
						.Parameter(typeof(string), "title")
						.Parameter(typeof(string), "author")
						.Parameter(typeof(decimal), "price")
						.Parameter(typeof(bool), "paperBack")
						;
					{
						g.Invoke(list, "Add", Exp.New(book, m, g.Arg("title"), g.Arg("author"), g.Arg("price"), g.Arg("paperBack")));
					}

					// Call a passed-in delegate on each paperback book to process it: 
					g = BookDBLocal.Public.Method(typeof(void), "ProcessPaperbackBooks").Parameter(processBookDelegate, "processBook");
					{
                        var b = g.ForEach(book, list);
						{
							g.If(b.Field("Paperback"));
							{
								g.InvokeDelegate(g.Arg("processBook"), b);
							}
							g.End();
						}
						g.End();
					}
				}
			}

			// Using the Bookstore classes:
			using (ag.Namespace("BookTestClient"))
			{
				// Class to total and average prices of books:
				TypeGen priceTotaller = ag.Class("PriceTotaller");
				{
					FieldGen countBooks = priceTotaller.Field(typeof(int), "countBooks", 0);
					FieldGen priceBooks = priceTotaller.Field(typeof(decimal), "priceBooks", 0.0m);

					CodeGen g = priceTotaller.Internal.Method(typeof(void), "AddBookToTotal").Parameter(book, "book");
					{
						g.AssignAdd(countBooks, 1);
						g.AssignAdd(priceBooks, g.Arg("book").Field("Price"));
					}

					g = priceTotaller.Internal.Method(typeof(decimal), "AveragePrice");
					{
						g.Return(priceBooks / countBooks);
					}
				}

				// Class to test the book database:
				TypeGen test = ag.Class("Test");
				{
					// Print the title of the book.
					CodeGen g = test.Static.Method(typeof(void), "PrintTitle").Parameter(book, "book");
					{
						g.WriteLine("   {0}", g.Arg("book").Field("Title"));
					}

					// Initialize the book database with some test books:
					g = test.Static.Method(typeof(void), "AddBooks").Parameter(BookDBLocal, "bookDB");
					{
                        var bookDb = g.Arg("bookDB");

						g.Invoke(bookDb, "AddBook", "The C Programming Language",
						  "Brian W. Kernighan and Dennis M. Ritchie", 19.95m, true);
						g.Invoke(bookDb, "AddBook", "The Unicode Standard 2.0",
						   "The Unicode Consortium", 39.95m, true);
						g.Invoke(bookDb, "AddBook", "The MS-DOS Encyclopedia",
						   "Ray Duncan", 129.95m, false);
						g.Invoke(bookDb, "AddBook", "Dogbert's Clues for the Clueless",
						   "Scott Adams", 12.00m, true);
					}

					// Execution starts here.
					g = test.Public.Static.Method(typeof(void), "Main");
					{
                        var bookDb = g.Local(Exp.New(BookDBLocal, m));

						// Initialize the database with some books:
						g.Invoke(test, "AddBooks", bookDb);

						// Print all the titles of paperbacks:
						g.WriteLine("Paperback Book Titles:");
						// Create a new delegate object associated with the static 
						// method Test.PrintTitle:
						g.Invoke(bookDb, "ProcessPaperbackBooks", Exp.NewDelegate(processBookDelegate, test, "PrintTitle", m));

                        // Get the average price of a paperback by using
                        // a PriceTotaller object:
                        var totaller = g.Local(Exp.New(priceTotaller, m));
						// Create a new delegate object associated with the nonstatic 
						// method AddBookToTotal on the object totaller:
						g.Invoke(bookDb, "ProcessPaperbackBooks", Exp.NewDelegate(processBookDelegate, totaller, "AddBookToTotal", m));
						g.WriteLine("Average Paperback Book Price: ${0:#.##}",
						   totaller.Invoke("AveragePrice"));
					}
				}
			}
		}

		// example based on the MSDN Delegates Sample (compose.cs)
		public static void GenCompose(AssemblyGen ag)
		{
			TypeGen myDelegate = ag.Delegate(typeof(void), "MyDelegate").Parameter(typeof(string), "string");

			TypeGen myClass = ag.Class("MyClass");
			{
				CodeGen g = myClass.Public.Static.Method(typeof(void), "Hello").Parameter(typeof(string), "s");
				{
					g.WriteLine("  Hello, {0}!", g.Arg("s"));
				}

				g = myClass.Public.Static.Method(typeof(void), "Goodbye").Parameter(typeof(string), "s");
				{
					g.WriteLine("  Goodbye, {0}!", g.Arg("s"));
				}

				g = myClass.Public.Static.Method(typeof(void), "Main");
				{
					ContextualOperand a = g.Local(), b = g.Local(), c = g.Local(), d = g.Local();

					// Create the delegate object a that references 
					// the method Hello:
					g.Assign(a, Exp.NewDelegate(myDelegate, myClass, "Hello", ag.TypeMapper));
					// Create the delegate object b that references 
					// the method Goodbye:
					g.Assign(b, Exp.NewDelegate(myDelegate, myClass, "Goodbye", ag.TypeMapper));
					// The two delegates, a and b, are composed to form c, 
					// which calls both methods in order:
					g.Assign(c, a + b);
					// Remove a from the composed delegate, leaving d, 
					// which calls only the method Goodbye:
					g.Assign(d, c - a);

					g.WriteLine("Invoking delegate a:");
					g.InvokeDelegate(a, "A");
					g.WriteLine("Invoking delegate b:");
					g.InvokeDelegate(b, "B");
					g.WriteLine("Invoking delegate c:");
					g.InvokeDelegate(c, "C");
					g.WriteLine("Invoking delegate d:");
					g.InvokeDelegate(d, "D");
				}
			}
		}
	}
}
