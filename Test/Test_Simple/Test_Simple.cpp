// created by i-saint
// distributed under Creative Commons Attribution (CC BY) license.
// https://github.com/i-saint/DynamicPatcher

#include <windows.h>
#include <intrin.h>
#include <stdint.h>
#include <cstdio>
#include <clocale>
#include <algorithm>



class Test
{
public:
    Test() : m_end_flag(false) {}
    virtual ~Test() {}

    virtual void doSomething()
    {
        puts("Test::doSomething()");
        printf("Test::s_value: %d\n", s_value);
        //m_end_flag = true;
    }

    bool getEndFlag() const { return m_end_flag; }

private:
    static int s_value;
    static const int s_cvalue;
    bool m_end_flag;
};
int Test::s_value = 42;
const int Test::s_cvalue = 42;

void Test_ThisMaybeOverridden()
{
    printf("Test_ThisMaybeOverridden()\n");
}

int main(int argc, char *argv[])
{
    printf("DynamicPatcher Test_Simple\n");
    {
        Test test;
        while(!test.getEndFlag()) {
            test.doSomething();

            ::Sleep(1000);
        }
    }
}

